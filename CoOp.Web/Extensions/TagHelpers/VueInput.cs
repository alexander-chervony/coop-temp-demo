/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;


namespace CoOp.Web.Extensions.TagHelpers
{

    /// <summary>
    /// In case of performance issues localizer caching can be done as here
    /// https://github.com/WormieCorp/Localization.AspNetCore.TagHelpers/blob/develop/src/Localization.AspNetCore.TagHelpers/GenericLocalizeTagHelper.cs
    /// </summary>
    [HtmlTargetElement("v-input")]
    public class VueInput : TagHelper
    {
        private readonly IStringLocalizer _localizer;

        public VueInput(IStringLocalizerFactory factory, IUrlHelper urlHelper)
        {
            var controller = urlHelper.ActionContext.RouteData.Values["Controller"];
            var action = urlHelper.ActionContext.RouteData.Values["Action"];
            var str = $"Views.{controller}.{action}";
            _localizer = factory.Create(str,
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
        }

        [HtmlAttributeName("id")]
        public string Id { get; set; }

        [HtmlAttributeName("type")]
        public string Type { get; set; }

        [HtmlAttributeName("label-cols")] 
        public string LabelCols { get; set; } = "4";

        [HtmlAttributeName("min")] 
        public string Min { get; set; }

        [HtmlAttributeName("max")] 
        public string Max { get; set; }
        
        [HtmlAttributeName("step")] 
        public string Step { get; set; }

        [HtmlAttributeName("description")] 
        public bool Description { get; set; } = false;
        
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string Localize(string s) => _localizer[Id + s].Value;
            var type = Type == null ? "" : $@"type=""{Type}""";
            var rangeParams = new StringBuilder();
            var description = Description ? $@" :description=""m.{Id}.toString()""" : "";
            if ( Type != null && Type.ToLower().Equals("range") )
            {
                rangeParams.Append(Min  != null ? $@" :min=""{Min}""" : "");
                rangeParams.Append(Max  != null ? $@" :max=""{Max}""" : "");
                rangeParams.Append(Step != null ? $@" :step=""{Step}""" : "");
            }

            var template = $@"
<b-form-group label-cols-sm=""{LabelCols}"" label=""{Localize(".Label")}"" label-align-sm=""right"" label-for=""{Id}"" {description}>
    <b-form-input id=""{Id}"" 
        v-model=""m.{Id}"" 
        :state=""validationState('{Id}')"" {type} {rangeParams}
        aria-describedby=""{Id}-feedback"" 
        placeholder=""{Localize(".Placeholder")}""
        trim>
    </b-form-input>
    <b-form-invalid-feedback id=""{Id}-feedback"" ><template v-for=""err in Errors.{Id}"">{{{{err}}}}<br/></template></b-form-invalid-feedback>
</b-form-group>
";
            output.TagName = null;
            output.Content.SetHtmlContent(template);
        }
    }
}
