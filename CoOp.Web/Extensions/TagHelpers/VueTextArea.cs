/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;


namespace CoOp.Web.Extensions.TagHelpers
{

    /// <summary>
    /// In case of performance issues localizer caching can be done as here
    /// https://github.com/WormieCorp/Localization.AspNetCore.TagHelpers/blob/develop/src/Localization.AspNetCore.TagHelpers/GenericLocalizeTagHelper.cs
    /// </summary>
    [HtmlTargetElement("v-textarea")]
    public class VueTextArea : TagHelper
    {
        private readonly IStringLocalizer _localizer;

        public VueTextArea(IStringLocalizerFactory factory, IUrlHelper urlHelper)
        {
            var controller = urlHelper.ActionContext.RouteData.Values["Controller"];
            var action = urlHelper.ActionContext.RouteData.Values["Action"];
            var str = $"Views.{controller}.{action}";
            _localizer = factory.Create(str,
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);

        }

        [HtmlAttributeName("id")]
        public string Id { get; set; }

        [HtmlAttributeName("label-cols")] 
        public string LabelCols { get; set; } = "4";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string Localize(string s) => _localizer[Id + s].Value;
          
            var template = $@"
<b-form-group label-cols-sm=""{LabelCols}"" label=""{Localize(".Label")}"" label-align-sm=""right"" label-for=""{Id}"">
    <b-form-textarea id=""{Id}"" 
        v-model=""m.{Id}"" 
        :state=""validationState('{Id}')""
        aria-describedby=""{Id}-feedback"" 
        placeholder=""{Localize(".Placeholder")}""
        >
    </b-form-textarea>
    <b-form-invalid-feedback id=""{Id}-feedback""><template v-for=""err in Errors.{Id}"">{{{{err}}}}<br/></template></b-form-invalid-feedback>
</b-form-group>
";
            output.TagName = null;
            output.Content.SetHtmlContent(template);
        }
    }
}
