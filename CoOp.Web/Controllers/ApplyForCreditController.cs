/*******************************************************
 * Copyright (C) 2020-2021 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoOp.Domain;
using CoOp.Web.Controllers.Filters;
using CoOp.Web.Models.ApplyForCredit;
using CoOp.Web.Models.Calc;
using EventFlow.Extensions;
using EventFlow.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CoOp.Web.Controllers
{
    public class ApplyForCreditController : Controller
    {
        private readonly IQueryProcessor _queryProcessor;
        private readonly IStringLocalizer _localizer;

        public ApplyForCreditController(IQueryProcessor queryProcessor, IStringLocalizerFactory factory)
        {
            _queryProcessor = queryProcessor;
            _localizer = factory.Create("Views.ApplyForCredit.Index", 
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);;
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [AjaxValidation]
        public IActionResult SendApplication([FromBody] ApplyForCreditInputModel i)
        {
            var t = i.BirthDate;
            var message = new MimeMessage();
            var from = new MailboxAddress("ООО Алло", "allo.gomel@gmail.com");
            message.From.Add(from);
            var to = new MailboxAddress("Оформитель Заявок", "oformitel.zayavok@gmail.com");
            message.To.Add(to);

            message.Subject = $"[{i.SumToCredit} руб] {i.Fio}";

            var bodyBuilder = new BodyBuilder {HtmlBody = FormatData(i)};
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTlsWhenAvailable);
                client.Authenticate("sergixc@gmail.com", "bbuoykgiflouutei");
                client.Send(message);
                client.Disconnect(true);
            }

            return Json(i);
        }

        public string FormatData(ApplyForCreditInputModel i)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<h1>Заявка на кредит</h1>");
            sb.AppendLine("<ul>");
            
            sb.AppendLine($"<li>{_localizer["Fio.Label"]}: {i.Fio}</li>");
            sb.AppendLine($"<li>{_localizer["LastNameOld.Label"]}: {i.LastNameOld}</li>");                
            sb.AppendLine($"<li>{_localizer["CellPhoneNumber.Label"]}: {i.CellPhoneNumber}</li>");            
            sb.AppendLine($"<li>{_localizer["DocumentType.Label"]}: {i.DocumentType}</li>");               
            sb.AppendLine($"<li>{_localizer["BirthDate.Label"]}: {i.BirthDate}</li>");                  
            sb.AppendLine($"<li>{_localizer["PassportSn.Label"]}: {i.PassportSn}</li>");                 
            sb.AppendLine($"<li>{_localizer["PassportPn.Label"]}: {i.PassportPn}</li>");                 
            sb.AppendLine($"<li>{_localizer["PassportIssuedBy.Label"]}: {i.PassportIssuedBy}</li>");           
            sb.AppendLine($"<li>{_localizer["PassportIssuedDate.Label"]}: {i.PassportIssuedDate}</li>");         
            sb.AppendLine($"<li>{_localizer["PassportExpDate.Label"]}: {i.PassportExpDate}</li>");            
            sb.AppendLine($"<li>{_localizer["Address.Label"]}: {i.Address}</li>");                    
            sb.AppendLine($"<li>{_localizer["Product.Label"]}: {i.Product}</li>");                    
            sb.AppendLine($"<li>{_localizer["SumToCredit.Label"]}: {i.SumToCredit}</li>");                
            sb.AppendLine($"<li>{_localizer["Period.Label"]}: {i.Period}</li>");                     
            sb.AppendLine($"<li>{_localizer["LivingIn.Label"]}: {i.LivingIn}</li>");                   
            sb.AppendLine($"<li>{_localizer["LivingAddress.Label"]}: {i.LivingAddress}</li>");              
            sb.AppendLine($"<li>{_localizer["Education.Label"]}: {i.Education}</li>");                  
            sb.AppendLine($"<li>{_localizer["MaritalStatus.Label"]}: {i.MaritalStatus}</li>");              
            sb.AppendLine($"<li>{_localizer["MaritalPartnerName.Label"]}: {i.MaritalPartnerName}</li>");         
            sb.AppendLine($"<li>{_localizer["MaritalPartnerCell.Label"]}: {i.MaritalPartnerCell}</li>");         
            sb.AppendLine($"<li>{_localizer["ChildrenUnder18.Label"]}: {i.ChildrenUnder18}</li>");            
            sb.AppendLine($"<li>{_localizer["OrganizationType.Label"]}: {i.OrganizationType}</li>");           
            sb.AppendLine($"<li>{_localizer["OrganizationNameAndAddress.Label"]}: {i.OrganizationNameAndAddress}</li>"); 
            sb.AppendLine($"<li>{_localizer["JobTitle.Label"]}: {i.JobTitle}</li>");                   
            sb.AppendLine($"<li>{_localizer["JobExpirienceLast.Label"]}: {i.JobExpirienceLast}</li>");          
            sb.AppendLine($"<li>{_localizer["JobExpirienceTotal.Label"]}: {i.JobExpirienceTotal}</li>");         
            sb.AppendLine($"<li>{_localizer["JobPartnerNameAndCell.Label"]}: {i.JobPartnerNameAndCell}</li>");      
            sb.AppendLine($"<li>{_localizer["IncomeAverage.Label"]}: {i.IncomeAverage}</li>");              
            sb.AppendLine($"<li>{_localizer["HaveACar.Label"]}: {_localizer[$"SelectValue."+i.HaveACar]}</li>");                   
            sb.AppendLine($"<li>{_localizer["HaveAProperty.Label"]}: {_localizer[$"SelectValue."+i.HaveAProperty]}</li>");              
            sb.AppendLine($"<li>{_localizer["PropertyType.Label"]}: {i.PropertyType}</li>");               
            sb.AppendLine($"<li>{_localizer["HaveActiveCredit.Label"]}: {_localizer[$"SelectValue."+i.HaveActiveCredit]}</li>");           
            sb.AppendLine($"<li>{_localizer["MonthlyPaymentCredit.Label"]}: {i.MonthlyPaymentCredit}</li>");       
            sb.AppendLine($"<li>{_localizer["RelativeNameAndCell.Label"]}: {i.RelativeNameAndCell}</li>");        
            sb.AppendLine($"<li>{_localizer["Passphrase.Label"]}: {i.Passphrase}</li>");                  
            sb.AppendLine("</ul>");
            return sb.ToString();
        }
    }
}