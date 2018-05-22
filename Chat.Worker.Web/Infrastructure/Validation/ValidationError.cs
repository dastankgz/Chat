using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Chat.Worker.Web.Infrastructure.Validation
{
    public class ValidationError
    {
        public string ParamName { get; set; }
        
        public string EntityType { get; set; }

        public dynamic Errors { get; set; }
    }

    internal static class ValidationErrorsFactory
    {
        public static ValidationError Build(string paramName, object invalidObject, List<ValidationResult> errs)
        {
            ValidationError obj = new ValidationError();

            obj.ParamName = paramName;

            obj.EntityType = invalidObject.GetType().ToString();

            dynamic errors = new JObject();

            foreach (var err in errs)
            {
                errors[err.MemberNames.FirstOrDefault() ?? ""] = err.ErrorMessage;
            }

            obj.Errors = errors;

            return obj;
        }

        public static ValidationError BuildNull(string paramName)
        {
            return new ValidationError
            {
                ParamName = paramName,
                EntityType = "null"
            };
        }
    }
}