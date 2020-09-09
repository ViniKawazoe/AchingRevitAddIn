#region namespaces
using System.Collections.Generic;
using Autodesk.Revit.DB;
#endregion

/// <summary>
/// This class was adapted from "The Building Coder" Blog
/// Link: https://thebuildingcoder.typepad.com/blog/2012/04/failure-rollback.html
/// If the failure is a warning, it deletes de the warning
/// If the failure is another type, it will rollback the operation and keep going
/// </summary>
namespace AchingRevitAddIn
{
    public class FailureHandler : IFailuresPreprocessor
    {
        public string ErrorMessage { get; set; }
        public string ErrorSeverity { get; set; }

        public FailureHandler()
        {
            ErrorMessage = "";
            ErrorSeverity = "";
        }

        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            IList<FailureMessageAccessor> failureMessages = failuresAccessor.GetFailureMessages();

            foreach (FailureMessageAccessor failureMessageAccessor in failureMessages)
            {
                // Delete all warnings and rolling back other failures
                FailureDefinitionId id = failureMessageAccessor.GetFailureDefinitionId();

                try
                {
                    ErrorMessage = failureMessageAccessor.GetDescriptionText();
                }
                catch
                {
                    ErrorMessage = "Unknown Error";
                }

                try
                {
                    FailureSeverity failureSeverity = failureMessageAccessor.GetSeverity();
                    ErrorSeverity = failureSeverity.ToString();

                    if (failureSeverity == FailureSeverity.Warning)
                    {
                        failuresAccessor.DeleteWarning(failureMessageAccessor);
                    }
                    else
                    {
                        return FailureProcessingResult.ProceedWithRollBack;
                    }
                }
                catch
                {
                }
            }
            return FailureProcessingResult.Continue;
        }
    }
}
