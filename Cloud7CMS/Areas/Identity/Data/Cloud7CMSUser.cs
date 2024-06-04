using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Cloud7CMS.Areas.Identity.Data;

// Add profile data for application users by adding properties to the Cloud7CMSUser class
public class Cloud7CMSUser : IdentityUser
{
	// Additional properties can be added here
	// For example:
	// public string FullName { get; set; }
}

