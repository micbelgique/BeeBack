﻿using System.Web.Http;
using BeeBack.Web.Auth.BasicAuth.Filters;

namespace BeeBack.Web.Controllers.Api
{
    [IdentityBasicAuthentication]
    public class BaseApiController: ApiController
    {
    }
}