﻿using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Newtonsoft.Json;

namespace System.Web.Mvc
{
    /// <summary>
    /// MVC HtmlHelper extension methods - extensions that make use of jQuery Validates native unobtrusive data validation properties
    /// </summary>
    public static class Mapper
    {
        public static RouteValueDictionary GetUnobtrusiveValidationAttributes<TModel, TProperty>(
            HtmlHelper<TModel> helper,
            Expression<Func<TModel, TProperty>> expression,
            object htmlAttributes,
            ModelMetadata metadata)
        {
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

            // write rules to context object
            var validators = ModelValidatorProviders.Providers.GetValidators(metadata, helper.ViewContext);
            foreach (var rule in validators.SelectMany(v => v.GetClientValidationRules()))
            {
                // if we have an error message for this rule then create a data-msg-/rulename/ attribute to store it
                if (!string.IsNullOrEmpty(rule.ErrorMessage) && rule.ValidationType != "length") // length is the only exception to this standard rule
                    attributes.Add("data-msg-" + rule.ValidationType, rule.ErrorMessage);

                switch (rule.ValidationType)
                {
                    // Exceptions to the standard handling of rules

                    case "equalto":
                        // Convert equalto selector from "*.PasswordDemo" style to "#PasswordDemo" style
                        var equalToSelector = "#" + rule.ValidationParameters["other"].ToString().Substring(2);
                        attributes.Add("data-rule-equalto", equalToSelector);
                        break;

                    case "length":
                        // length is mapped to minlength and maxlength
                        if (rule.ValidationParameters.ContainsKey("min"))
                        {
                            attributes.Add("data-rule-minlength", rule.ValidationParameters["min"]);
                            attributes.Add("data-msg-minlength", rule.ErrorMessage);
                        }

                        if (rule.ValidationParameters.ContainsKey("max"))
                        {
                            attributes.Add("data-rule-maxlength", rule.ValidationParameters["max"]);
                            attributes.Add("data-msg-maxlength", rule.ErrorMessage);
                        }
                        break;

                    case "range":
                        attributes.Add("data-rule-range", string.Format("[{0},{1}]", rule.ValidationParameters["min"], rule.ValidationParameters["max"]));
                        break;

                    // Standard handling of rules

                    default:

                        // if we have *no* validation parameters then create a data-rule-/rulename/ attribute with the value "true"
                        // if we have validation parameters then create a data-rule-/rulename/ attribute with the JSON'd ValidationParameters
                        attributes.Add("data-rule-" + rule.ValidationType,
                            rule.ValidationParameters.Count == 0
                                ? "true"
                                : JsonConvert.SerializeObject(rule.ValidationParameters));
                        break;
                }
            }

            //MapMvcUnobtrusiveValidationAttributesToNative(helper, expression, metadata, attributes);

            return attributes;
        }

        public static IHtmlString GenerateHtmlWithoutMvcUnobtrusiveAttributes(Func<IHtmlString> generator)
        {
            var cachedClientValidationEnabled = HtmlHelper.ClientValidationEnabled;
            var cachedUnobtrusiveJavaScriptEnabled = HtmlHelper.UnobtrusiveJavaScriptEnabled;

            HtmlHelper.ClientValidationEnabled = false;
            HtmlHelper.UnobtrusiveJavaScriptEnabled = false;

            var html = generator();

            HtmlHelper.ClientValidationEnabled = cachedClientValidationEnabled;
            HtmlHelper.UnobtrusiveJavaScriptEnabled = cachedUnobtrusiveJavaScriptEnabled;

            return html;
        }

        private const string data_val_required = "data-val-required";

        private const string data_val_number = "data-val-number";
        private const string data_val_date = "data-val-date";
        private const string data_val_email = "data-val-email";
        private const string data_val_url = "data-val-url";
        private const string data_val_creditcard = "data-val-creditcard";
        private const string data_val_equalto = "data-val-equalto";
        private const string data_val_remote = "data-val-remote";
        private const string data_val_remote_additionalfields = "data-val-remote-additionalfields";

        private const string data_val_length = "data-val-length";
        private const string data_val_length_min = "data-val-length-min";
        private const string data_val_length_max = "data-val-length-max";
        
        private const string data_val_range = "data-val-range";

        /// <summary>
        /// Take the native MVC generated unobtrusive validation attributes and map them to the equivalent native attributes
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="helper"></param>
        /// <param name="expression"></param>
        /// <param name="metadata"></param>
        /// <param name="attributes"></param>
        private static void MapMvcUnobtrusiveValidationAttributesToNative<TModel, TProperty>(
            HtmlHelper<TModel> helper,
            Expression<Func<TModel, TProperty>> expression,
            ModelMetadata metadata,
            RouteValueDictionary attributes)
        {
            var propertyName = helper.NameFor(expression).ToString(); //var propertyName = ModelMetadata.FromLambdaExpression(expression, helper.ViewData).PropertyName; // MVC 3 can use this
            var unobtrusiveValidationAttributes = helper.GetUnobtrusiveValidationAttributes(propertyName, metadata);

            // Number
            // If a property is an Short, Integer, Long, Byte, Decimal, Single or Double then this validation will be
            // applied (and also if they are Nullables of the same types)
            if (unobtrusiveValidationAttributes.ContainsKey(data_val_number))
            {
                attributes.Add("data-rule-number", "true");
                attributes.Add("data-msg-number", unobtrusiveValidationAttributes[data_val_number]);
            }

            // Date
            // If a property is a DateTime (or similar) then this validation will be
            // applied (and also if they are Nullables of the same types)
            if (unobtrusiveValidationAttributes.ContainsKey(data_val_date))
            {
                attributes.Add("data-rule-date", "true");
                attributes.Add("data-msg-date", unobtrusiveValidationAttributes[data_val_date]);
            }

            // Email
            // Driven from EmailAddressAttribute eg "[EmailAddress]"
            if (unobtrusiveValidationAttributes.ContainsKey(data_val_email))
            {
                attributes.Add("data-rule-email", "true");
                attributes.Add("data-msg-email", unobtrusiveValidationAttributes[data_val_email]);
            }


            // Url
            // Driven from UrlAttribute eg "[Url]"
            if (unobtrusiveValidationAttributes.ContainsKey(data_val_url))
            {
                attributes.Add("data-rule-url", "true");
                attributes.Add("data-msg-url", unobtrusiveValidationAttributes[data_val_url]);
            }

            // CreditCard
            // Driven from CreditCardAttribute eg "[CreditCard]"
            if (unobtrusiveValidationAttributes.ContainsKey(data_val_creditcard))
            {
                attributes.Add("data-rule-creditcard", "true");
                attributes.Add("data-msg-creditcard", unobtrusiveValidationAttributes[data_val_creditcard]);
            }

            // EqualTo
            // Driven from CompareAttribute eg "[Compare("PasswordDemo", ErrorMessage = "Passwords do not match.")]"
            if (unobtrusiveValidationAttributes.ContainsKey(data_val_equalto))
            {
                // Convert equalto selector from "*.PasswordDemo" style to "#PasswordDemo" style
                var equalToSelector = unobtrusiveValidationAttributes["data-val-equalto-other"].ToString();
                equalToSelector = "#" + equalToSelector.Substring(2);

                attributes.Add("data-rule-equalto", equalToSelector);
                attributes.Add("data-msg-equalto", unobtrusiveValidationAttributes[data_val_equalto]);
            }

            // Required
            // Driven from RequiredAttribute eg "[Required]"
            if (unobtrusiveValidationAttributes.ContainsKey(data_val_required))
            {
                attributes.Add("data-rule-required", "true");
                attributes.Add("data-msg-required", unobtrusiveValidationAttributes[data_val_required]);
            }

            // Min and Max Length
            // Driven from StringLengthAttribute eg "[StringLength(10, MinimumLength=5)]"
            if (unobtrusiveValidationAttributes.ContainsKey(data_val_length))
            {
                if (unobtrusiveValidationAttributes.ContainsKey(data_val_length_min))
                {
                    attributes.Add("data-rule-minlength", unobtrusiveValidationAttributes[data_val_length_min]);
                    attributes.Add("data-msg-minlength", unobtrusiveValidationAttributes[data_val_length]);
                }

                if (unobtrusiveValidationAttributes.ContainsKey(data_val_length_max))
                {
                    attributes.Add("data-rule-maxlength", unobtrusiveValidationAttributes[data_val_length_max]);
                    attributes.Add("data-msg-maxlength", unobtrusiveValidationAttributes[data_val_length]);
                }
            }

            // Range
            // Driven from RangeAttribute eg "[Range(0, 100)]"
            if (unobtrusiveValidationAttributes.ContainsKey(data_val_range))
            {
                attributes.Add("data-rule-range",
                               String.Format("[{0},{1}]",
                                             unobtrusiveValidationAttributes["data-val-range-min"],
                                             unobtrusiveValidationAttributes["data-val-range-max"])
                    );
                attributes.Add("data-msg-range", unobtrusiveValidationAttributes[data_val_range]);
            }

            // Remote
            // Driven from RemoteAttribute eg "[Remote("Remote", "Demo")]"
            if (unobtrusiveValidationAttributes.ContainsKey(data_val_remote))
            {
                if (unobtrusiveValidationAttributes[data_val_remote_additionalfields].ToString().Contains(","))
                {
                    // THIS MECHANISM DOESN'T REALLY WORK AT PRESENT - ADDITIONAL FIELDS ARE NOT WELL
                    // CATERED FOR YET WITH jQuery.Validate
                    // TODO: COME BACK TO WHEN A CLEAR APPROACH WITH jQuery.Validate IS AVAILABLE
                    attributes.Add("data-rule-remote", "{" +
                                                       "\"url\": \"" + unobtrusiveValidationAttributes["data-val-remote-url"] +
                                                       "\"," +
                                                       "\"type\": \"post\"," +
                                                       "\"data\": {" +
                                                       "\"Simple\": \"function() { return $('#Simple').val(); }\"" +
                                                       "}" +
                                                       "}");

                    /* what the dynamically generated attrs look like:
    unobtrusiveValidationAttributes
    Count = 4
        [0]: {[data-val-remote, 'AdditionalFields' is invalid.]}
        [1]: {[data-val-remote-url, /Demo/RemoteAdditionalFields]}
        [2]: {[data-val-remote-additionalfields, *.AdditionalFields,*.Simple]}
        [3]: {[data-val, true]}
                     */
                }
                else
                    attributes.Add("data-rule-remote", unobtrusiveValidationAttributes["data-val-remote-url"]);

                attributes.Add("data-msg-remote", unobtrusiveValidationAttributes[data_val_remote]);
            }
        }
    }
}
