using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NooSphere.Core.ContextModel;
using NooSphere.Core.Primitives;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Xml;
using System.Security.Principal;
using NooSphere.Helpers;

namespace NooSphere.Core.ActivityModel
{
    public class Participant : IEntity, IPrincipal
    {
        #region Constructors
        public Participant()
        {
            InitializeProperties();
        }
        #endregion

        #region Initializers
        private void InitializeProperties()
        {
            this.Identity = new Identity();
            this._participantIdentity = new ParticipantIdentity("Name", "Type");
        }
        #endregion

        #region Properties
        private Identity _identity;
        public Identity Identity
        {
            get { return _identity; }
            set { this._identity = value; }
        }
        private object _image;
        public object Image
        {
            get { return _image; }
            set { this._image = value; }
        }
        public string AccessTokenExpirationDate { get; set; }
        public string AccessToken { get; set; }
        public string Email { get; set; }
        public string SWTToken
        {
            set { ValidateAccessToken(value); }
        }
        #endregion

        #region IPrincipal Interface
        private ParticipantIdentity _participantIdentity;
        IIdentity IPrincipal.Identity
        {
            get { return _participantIdentity; }
        }
        bool IPrincipal.IsInRole(string role)
        {
            return true;
        }
        #endregion

        private void ValidateAccessToken(string swtToken)
        {
            JObject root = JObject.Parse(swtToken);

            // Set token expiration date
            AccessTokenExpirationDate = HttpUtility.HtmlDecode(root["expires"].ToString());

            // Set access token
            string securityToken = HttpUtility.HtmlDecode(root["securityToken"].ToString());
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(securityToken);
            var swtBuffer = Convert.FromBase64String(doc.DocumentElement.InnerText);
            AccessToken = Encoding.Default.GetString(swtBuffer);

            Dictionary<string, string> tokenValues = TokenHelper.GetNameValues(AccessToken);
            // Set eamil
            string email;
            tokenValues.TryGetValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", out email);
            Email = email;
            // Set name
            string name;
            tokenValues.TryGetValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", out name);
            Identity.Name = name;
        }
    }

    public class ParticipantIdentity : GenericIdentity
    {
        public ParticipantIdentity(string name, string type)
            : base(name, type)
        {

        }

    }
}
