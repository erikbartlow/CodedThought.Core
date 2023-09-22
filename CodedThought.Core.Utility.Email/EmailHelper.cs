using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CodedThought.Core.Utility.Email {

	public class EmailHelper {

		#region Properties

		public List<EmailAddress> ReceiverEmail { get; set; }

		public List<EmailAddress> CCEmail { get; set; }

		public EmailAddress SenderEmail { get; set; }

		public string Subject { get; set; }

		public string SMTPService { get; set; }

		public int Port { get; set; }

		public string TemplateHTML { get; set; }

		public List<EmailHelperAttachment> Attachments { get; set; }

		private IRazorViewEngine _viewEngine;
		private ITempDataProvider _tempDataProvider;
		private IServiceProvider _serviceProvider;

		#endregion Properties

		#region Constructor

		/// <summary>Default Constructor Console App</summary>
		public EmailHelper() {
			ReceiverEmail = new List<EmailAddress>();
			CCEmail = new List<EmailAddress>();
			this.Attachments = new List<EmailHelperAttachment>();
		}

		/// <summary>Constructor with all the information you need to send emails Console App</summary>
		/// <param name="ReceiverEmail">Email of the receiver</param>
		/// <param name="SenderEmail">  Email of the Sender</param>
		/// <param name="Subject">      Subject for the Email</param>
		/// <param name="smtp">         Service or Server to send the Email example: smtp3.CodedThought.com or SendGrid</param>
		public EmailHelper(List<EmailAddress> receiverEmail, EmailAddress senderEmail, string subject, string smtp) : this() {
			ReceiverEmail = new List<EmailAddress>();
			this.Attachments = new List<EmailHelperAttachment>();

			ReceiverEmail = receiverEmail;
			SenderEmail = senderEmail;
			SMTPService = smtp;
			Subject = subject;
		}

		/// <summary>Default Constructor</summary>
		public EmailHelper(IRazorViewEngine viewEngine,
			ITempDataProvider tempDataProvider,
			IServiceProvider serviceProvider) {
			_viewEngine = viewEngine;
			_tempDataProvider = tempDataProvider;
			_serviceProvider = serviceProvider;

			ReceiverEmail = new List<EmailAddress>();
			CCEmail = new List<EmailAddress>();
			this.Attachments = new List<EmailHelperAttachment>();
		}

		/// <summary>Constructor with all the information you need to send emails</summary>
		/// <param name="ReceiverEmail">Email of the receiver</param>
		/// <param name="SenderEmail">  Email of the Sender</param>
		/// <param name="Subject">      Subject for the Email</param>
		/// <param name="smtp">         Service or Server to send the Email example: smtp3.CodedThought.com or SendGrid</param>
		public EmailHelper(IRazorViewEngine viewEngine,
		ITempDataProvider tempDataProvider,
			IServiceProvider serviceProvider, List<EmailAddress> receiverEmail, EmailAddress senderEmail, string subject, string smtp) : this(viewEngine, tempDataProvider, serviceProvider) {
			ReceiverEmail = new List<EmailAddress>();
			this.Attachments = new List<EmailHelperAttachment>();

			ReceiverEmail = receiverEmail;
			SenderEmail = senderEmail;
			SMTPService = smtp;
			Subject = subject;
		}

		#endregion Constructor

		/// <summary>Method to Add an email to the list of receivers</summary>
		/// <param name="email">Email of receiver</param>
		public void AddReceiverEmail(EmailAddress email) {
			if (ReceiverEmail == null)
				ReceiverEmail = new List<EmailAddress>();
			ReceiverEmail.Add(email);
		}

		/// <summary>Adds the cc email.</summary>
		/// <param name="email">The email.</param>
		public void AddCCEmail(EmailAddress email) {
			if (CCEmail == null)
				CCEmail = new List<EmailAddress>();
			CCEmail.Add(email);
		}

		/// <summary>Method to render the elements into your template string</summary>
		/// <param name="TemplateHTML">Template of the email</param>
		/// <param name="elements">    Class with all the elements to put in your template.</param>
		public void RenderString(string TemplateHTML, object? elements = null) {
			try {
				this.TemplateHTML = (elements != null) ? PutParameterOnHTML(TemplateHTML, elements) : TemplateHTML;
			} catch (Exception ex) {
				throw ex;
			}
		}

		/// <summary>Method to render the elements into your template file</summary>
		/// <param name="filename">Path/FileName of the html template</param>
		/// <param name="elements">Class with all the elements to put in your template.</param>
		public void RenderFileToString(string filename, object elements = null) {
			try {
				TextFileReader file = new(filename);
				TemplateHTML = file.Read();
				TemplateHTML = (elements != null) ? PutParameterOnHTML(TemplateHTML, elements) : TemplateHTML;
			} catch (Exception ex) {
				throw ex;
			}
		}

		/// <summary>Method to render the elements into your template partial view</summary>
		/// <param name="viewName">Path/Name of the Partial View</param>
		/// <param name="elements">Class with all the elements to put in your template.</param>
		public async Task RenderPartialToString(string controlName, object viewData) {
			var actionContext = GetActionContext();
			IView partial = FindView(actionContext, controlName);

			using (var output = new StringWriter()) {
				var viewContext = new ViewContext(
					actionContext,
					partial,
						new ViewDataDictionary<object>(
							metadataProvider: new EmptyModelMetadataProvider(),
							modelState: new ModelStateDictionary()) {
							Model = viewData
						},
						new TempDataDictionary(
							actionContext.HttpContext,
							_tempDataProvider),
					output,
					new HtmlHelperOptions()
				);
				await partial.RenderAsync(viewContext);
				this.TemplateHTML = output.ToString();
			}
		}

		private IView FindView(ActionContext actionContext, string partialName) {
			var getPartialResult = _viewEngine.GetView(null, partialName, false);
			if (getPartialResult.Success) {
				return getPartialResult.View;
			}

			var findPartialResult = _viewEngine.FindView(actionContext, partialName, false);
			if (findPartialResult.Success) {
				return findPartialResult.View;
			}

			var searchedLocations = getPartialResult.SearchedLocations.Concat(findPartialResult.SearchedLocations);
			var errorMessage = string.Join(
				Environment.NewLine,
				new[] { $"Unable to find partial '{partialName}'. The following locations were searched:" }.Concat(searchedLocations)); ;

			throw new InvalidOperationException(errorMessage);
		}

		private ActionContext GetActionContext() {
			var httpContext = new DefaultHttpContext {
				RequestServices = _serviceProvider
			};

			return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
		}

		/// <summary>Method to send your email</summary>
		public void Send() {
			try {
				if (Validate()) {
					switch (SMTPService) {
						//case "SendGrid":
						//    EmailServiceSendGrid();
						//    break;
						default:
							EmailServiceSMTP();
							break;
					}
				} else {
					throw new Exception("You need to configure this library");
				}
			} catch (Exception ex) {
				throw ex;
			}
		}

		/// <summary>Method to send your email asynchronous</summary>
		public Task SendAsync() {
			try {
				if (Validate()) {
					switch (SMTPService) {
						//case "SendGrid":
						//    return EmailServiceSendGridAsync();
						default:
							return EmailServiceSMTPAsync();
					}
				} else {
					throw new Exception("You need to configure this library");
				}
			} catch (Exception ex) {
				throw ex;
			}
		}

		public string PutParametersOnSubject(string TemplateHTML, object elements) {
			try {
				return this.PutParameterOnHTML(TemplateHTML, elements);
			} catch (Exception ex) {
				throw ex;
			}
		}

		public void PutParameterOnObjectTemplate(string parameter_name, string value, object obj) {
			try {
				Type myType = obj.GetType();
				PropertyInfo pinfo = myType.GetProperty(parameter_name);
				pinfo.SetValue(obj, value, null);
			} catch (Exception ex) {
				throw ex;
			}
		}

		#region Private Methods

		private bool Validate() {
			if (ReceiverEmail == null || ReceiverEmail.Count == 0)
				throw new Exception("You did not assigned an email as receiver");
			else if (SenderEmail == null)
				throw new Exception("You did not assigned an email as sender");
			else if (string.IsNullOrEmpty(SMTPService))
				throw new Exception("You did not assigned an SMTP Service/Server");
			else return string.IsNullOrEmpty(TemplateHTML) ? throw new Exception("You did not render a template") : true;
		}

		private string PutParameterOnHTML(string TemplateHTML, object elements) {
			try {
				MatchCollection template_values = Regex.Matches(TemplateHTML, @"\{{([A-Za-z]+)\}}");

				List<String> parameters = new();

				foreach (Match t in template_values) {
					parameters.Add(t.Value.Replace("{", string.Empty).Replace("}", string.Empty));
				}

				foreach (string p in parameters) {
					Type myType = elements.GetType();
					PropertyInfo pinfo = myType.GetProperty(p);
					if (pinfo != null) {
						string value = pinfo.GetValue(elements, null).ToString();
						TemplateHTML = TemplateHTML.Replace("{{" + p + "}}", value);
					} else if (elements is JToken) {
						JToken token = (JToken)elements;
						string value = token.Value<string?>(p) ?? "EMPTY";
						TemplateHTML = TemplateHTML.Replace("{{" + p + "}}", value);
					}
				}

				return TemplateHTML;
			} catch (Exception ex) {
				throw ex;
			}
		}

		private void EmailServiceSMTP() {
			try {
				MailMessage message = new() {
					From = new MailAddress(SenderEmail.Address, SenderEmail.AccountName),
					Subject = Subject,
					IsBodyHtml = true
				};

				if (Attachments.Count > 0) {
					foreach (var a in Attachments.Where(x => !x.Inline).ToList()) {
						System.Net.Mail.Attachment attachment;
						attachment = new System.Net.Mail.Attachment(a.Pathfile);
						message.Attachments.Add(attachment);
					}

					if (Attachments.Where(x => x.Inline).Count() > 0)
						message.AlternateViews.Add(GetAlternateView());
				}

				if (message.AlternateViews.Count == 0)
					message.Body = TemplateHTML;

				ReceiverEmail.ForEach(r => message.To.Add(new MailAddress(r.Address)));
				CCEmail.ForEach(c => message.CC.Add(new MailAddress(c.Address)));

				SmtpClient client = new() { Host = SMTPService };
				if (Port > 0) client.Port = Port;
				client.Send(message);
			} catch (Exception ex) {
				throw ex;
			}
		}

		private async Task EmailServiceSMTPAsync() {
			try {
				MailMessage message = new() {
					From = new MailAddress(SenderEmail.Address, SenderEmail.AccountName),
					Subject = Subject,
					IsBodyHtml = true
				};

				if (Attachments.Count > 0) {
					foreach (var a in Attachments.Where(x => !x.Inline).ToList()) {
						System.Net.Mail.Attachment attachment;
						attachment = new System.Net.Mail.Attachment(a.Pathfile);
						message.Attachments.Add(attachment);
					}

					if (Attachments.Where(x => x.Inline).Count() > 0)
						message.AlternateViews.Add(GetAlternateView());
				}

				if (message.AlternateViews.Count == 0) message.Body = TemplateHTML;

				ReceiverEmail.ForEach(r => message.To.Add(new MailAddress(r.Address)));
				CCEmail.ForEach(c => message.CC.Add(new MailAddress(c.Address)));

				SmtpClient client = new() { Host = SMTPService };
				client.SendCompleted += (o, e) => {
					if (e.Cancelled) {
						throw new Exception("Send canceled");
					}
					if (e.Error != null) {
						throw new Exception(e.Error.ToString());
					}
				};
				await client.SendMailAsync(message);
			} catch (Exception ex) {
				throw ex;
			}
		}

		private AlternateView GetAlternateView() {
			try {
				AlternateView alternateView = AlternateView.CreateAlternateViewFromString(TemplateHTML, null, MediaTypeNames.Text.Html);
				foreach (var a in Attachments.Where(x => x.Inline).ToList()) {
					LinkedResource inline = new(a.Pathfile) {
						ContentId = a.AttachmentName
					};
					alternateView.LinkedResources.Add(inline);
				}
				return alternateView;
			} catch (Exception ex) {
				throw ex;
			}
		}

		#endregion Private Methods
	}

	public class EmailHelperAttachment {
		public bool Inline { get; set; }

		public string Pathfile { get; set; }

		public string AttachmentName { get; set; }
	}

	public class FakeController : Controller {

		public FakeController() {
		}
	}
}