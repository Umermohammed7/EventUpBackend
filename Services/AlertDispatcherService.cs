namespace BackendEventUp.Services
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.EntityFrameworkCore;
    using BackendEventUp.Models;
    using BackendEventUp.Services;

    public class AlertDispatcherService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public AlertDispatcherService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<Myctx>();
                var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

                var alerts = await context.Alerter
                    .Include(a => a.Utilisateur)
                    .Include(a => a.Evenement)
                    .Where(a => a.DateAlerte <= DateTime.Now && a.StatusAlerte == "EnAttente")
                    .ToListAsync();

                foreach (var alert in alerts)
                {
                    var subject = $"📢 Rappel : {alert.Evenement.nom_evenement}";
                    var body = $"Bonjour {alert.Utilisateur.nom_utilisateur},<br/><br/>" +
                               $"Ceci est un rappel pour l'événement : <strong>{alert.Evenement.nom_evenement}</strong>.<br/><br/>" +
                               $"📅 Date : {alert.DateAlerte:dd/MM/yyyy HH:mm}<br/>" +
                               $"📍 Lieu : {alert.Evenement.Organiser.FirstOrDefault()?.AdresseEvenement}<br/><br/>" +
                               $"✉️ Message personnalisé : {alert.MessageAlerte}<br/><br/>" +
                               $"À bientôt !<br/>EventUp";

                    await emailService.SendEmailAsync(alert.Utilisateur.email_utilisateur, subject, body);

                    alert.StatusAlerte = "Envoyée";
                }

                await context.SaveChangesAsync();

                // Attendre 1 minute avant de vérifier à nouveau
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

}
