using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Helpers
{
    public class ServiceOperationResult
    {
        #region Properties
        /// <summary>
        /// The status of the service operation.
        /// </summary>
        public ServiceOperationStatus Status { get; set; } = ServiceOperationStatus.Indeterminate;
        /// <summary>
        /// The service operation's error message.
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// The service operation's user-friendly error message.
        /// <para>This is essentially the <see cref="ErrorMessage"/> without any technical information.</para>
        /// </summary>
        public string UserFriendlyErrorMessage { get; set; }
        /// <summary>
        /// Indicates whether or not the service operation suceeded.
        /// </summary>
        public bool OperationSuceeded
        {
            get
            {
                return Status == ServiceOperationStatus.Succeeded;
            }
        }
        /// <summary>
        /// Indicates whether or not the service operation failed.
        /// </summary>
        public bool OperationFailed
        {
            get
            {
                return Status == ServiceOperationStatus.Failed;
            }
        }
        #endregion

        #region Constructor
        public ServiceOperationResult(ServiceOperationStatus status = ServiceOperationStatus.Indeterminate, string errorMessage = null)
        {
            Status = status;
            ErrorMessage = errorMessage;
        }
        #endregion

        #region Public methods
        public async Task ShowUserErrorMessage(IDialogCoordinator dialogCoordinator, object dialogContext)
        {
            if (!string.IsNullOrWhiteSpace(UserFriendlyErrorMessage))
            {
                await dialogCoordinator.ShowMessageAsync(dialogContext, "Operation Failed", UserFriendlyErrorMessage);
            }
        }
        #endregion
    }
}
