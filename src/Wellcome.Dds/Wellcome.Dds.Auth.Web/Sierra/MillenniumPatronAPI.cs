﻿using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Utils;
using Wellcome.Dds.Common;

namespace Wellcome.Dds.Auth.Web.Sierra
{
    public class MillenniumPatronAPI : IAuthenticationService
    {
        private HttpClient httpClient;
        private DdsOptions ddsOptions;

        public MillenniumPatronAPI(
            HttpClient httpClient,
            IOptions<DdsOptions> options)
        {
            this.httpClient = httpClient;
            this.ddsOptions = options.Value;
        }

        private readonly string pinVerifyUrlFormat;

        public MillenniumPatronAPI(string pinVerifyUrlFormat)
        {
            this.pinVerifyUrlFormat = pinVerifyUrlFormat;
        }

        public async Task<AuthenticationResult> Authenticate(string username, string password)
        {
            try
            {
                username = HttpUtility.UrlEncode(username);
                password = HttpUtility.UrlEncode(password);
                var reqUrl = String.Format(pinVerifyUrlFormat, username, password);
                var resText = await httpClient.GetStringAsync(reqUrl);
                var msg = HtmlUtils.TextOnly(resText).Trim();
                if ("RETCOD=0".Equals(msg))
                {
                    return new AuthenticationResult { Success = true };
                }
                return new AuthenticationResult 
                { 
                    Success = false, 
                    Message = FormatFailure(msg) 
                };
            }
            catch (Exception authEx)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Authentication Exception: " + authEx.Message
                };
            }
        }

        private string FormatFailure(string patronApiMessage)
        {
            const string universalMessage = "Your username and/or password is incorrect.";
            // If the patron's record is found but the PIN is incorrect:
            // RETCOD=1
            // ERRNUM=4
            // ERRMSG=Invalid patron PIN

            // If the patron's record is found but there is no PIN in the record:
            // RETCOD=2
            // ERRNUM=4
            // ERRMSG=Invalid patron PIN

            // If the patron's record is not found:
            // ERRNUM=1
            // ERRMSG=Requested record not found

            if (patronApiMessage.Contains("RETCOD=1") && patronApiMessage.Contains("ERRNUM=4"))
            {
                return universalMessage;
            }

            if (patronApiMessage.Contains("RETCOD=2") && patronApiMessage.Contains("ERRNUM=4"))
            {
                return universalMessage;
            }

            if (patronApiMessage.Contains("ERRNUM=1"))
            {
                return universalMessage;
            }

            return patronApiMessage;
        }
    }
}
