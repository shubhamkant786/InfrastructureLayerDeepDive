using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Notifications.Push
{
    public class FirebasePushNotificationService : INotificationService<PushMessage>, IDisposable
    {
        private readonly FirebaseApp _firebaseApp;
        private readonly ILogger<FirebasePushNotificationService> _logger;

        public FirebasePushNotificationService(IOptions<FirebaseOptions> options, ILogger<FirebasePushNotificationService> logger)
        {
            _logger = logger;
            var firebaseOptions = options.Value;

            _firebaseApp = FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(firebaseOptions.CredentialsJsonPath),
                ProjectId = firebaseOptions.ProjectId
            });
        }

        public async Task SendAsync(PushMessage message, CancellationToken cancellationToken)
        {
            var fcmMessage = new Message
            {
                Token = message.DeviceToken,
                Notification = new Notification
                {
                    Title = message.Title,
                    Body = message.Body
                },
                Data = message.Data.AsReadOnly()
            };

            var messaging = FirebaseMessaging.GetMessaging(_firebaseApp);
            var messageId = await messaging.SendAsync(fcmMessage, cancellationToken);

            _logger.LogInformation("Push notification sent to device {DeviceToken}, message ID {MessageId}",
                message.DeviceToken, messageId);
        }

        public void Dispose()
        {
            _firebaseApp.Delete();
        }
    }
}