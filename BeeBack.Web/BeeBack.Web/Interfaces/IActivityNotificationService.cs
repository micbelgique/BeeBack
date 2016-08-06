﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeBack.Web.Models;

namespace BeeBack.Web.Interfaces
{
    public interface IActivityNotificationService
    {
        void NotifySubscribedUsers(Activity activity);
    }
}
