﻿global using FluentAssertions;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Logging.Abstractions;
global using MongoDB.Bson.Serialization.Conventions;
global using Moq;
global using SOS.Lib.Cache;
global using SOS.Lib.Configuration.Shared;
global using SOS.Lib.Database;
global using SOS.Lib.Database.Interfaces;
global using SOS.Lib.Enums;
global using SOS.Lib.Managers;
global using SOS.Lib.Managers.Interfaces;
global using SOS.Lib.Models.Interfaces;
global using SOS.Lib.Models.Processed.Configuration;
global using SOS.Lib.Models.TaxonListService;
global using SOS.Lib.Models.TaxonTree;
global using SOS.Lib.Repositories.Processed;
global using SOS.Lib.Repositories.Resource;
global using SOS.Lib.Security;
global using SOS.Lib.Security.Interfaces;
global using SOS.Lib.Services;
global using SOS.Lib.Services.Interfaces;
global using SOS.UserStatistics.Api.IntegrationTests.Fixtures;
global using SOS.UserStatistics.Api.IntegrationTests.Helpers;
global using SOS.UserStatistics.Api.Managers;
global using SOS.UserStatistics.Api.Models;
global using SOS.UserStatistics.Api.Repositories.Interfaces;
global using System.Reflection;
global using Xunit;
