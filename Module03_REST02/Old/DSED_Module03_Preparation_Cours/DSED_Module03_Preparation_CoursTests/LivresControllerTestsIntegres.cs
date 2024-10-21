using System;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using DSED_Module03_Preparation_Cours;

namespace DSED_Module03_Preparation_CoursTests
{
    public class LivresControllerTestsIntegres : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public LivresControllerTestsIntegres(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public void Test1()
        {

        }
    }
}
