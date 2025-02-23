using System;
using System.Text.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ray.BiliBiliTool.Config;
using Ray.BiliBiliTool.Config.Options;
using Ray.BiliBiliTool.Console;
using Xunit;
using Ray.BiliBiliTool.Infrastructure;
using Microsoft.Extensions.Hosting;
using Ray.BiliBiliTool.Agent;

namespace ConfigTest
{
    public class UnitTest1
    {
        [Fact]
        public void WebProxyTest()
        {
            Program.CreateHost(new string[] { });
            string proxyAddress = Global.ConfigurationRoot["Security:WebProxy"];

            if (!proxyAddress.IsNullOrEmpty())
            {
                WebProxy webProxy = new WebProxy();

                //user:password@host:port http proxy only .Tested with tinyproxy-1.11.0-rc1
                if (proxyAddress.Contains("@"))
                {
                    string userPass = proxyAddress.Split("@")[0];
                    string address = proxyAddress.Split("@")[1];

                    string proxyUser = userPass.Split(":")[0];
                    string proxyPass = userPass.Split(":")[1];

                    webProxy.Address = new Uri("http://" + address);
                    webProxy.Credentials = new NetworkCredential(proxyUser, proxyPass);
                }
                else
                {
                    webProxy.Address = new Uri(proxyAddress);
                }

                HttpClient.DefaultProxy = webProxy;

                HttpClient httpClient = new HttpClient();
                var response = httpClient.GetAsync("http://api.ipify.org/");
                var resultIp = response.Result.Content.ReadAsStringAsync().Result;
                Debug.WriteLine(String.Format("��ǰIP�� {0}", resultIp));
            }
        }
        [Fact]
        public void Test1()
        {
            Program.CreateHost(new string[] { });

            string s = Global.ConfigurationRoot["BiliBiliCookie:UserId"];
            Debug.WriteLine(s);

            string logLevel = Global.ConfigurationRoot["Serilog:WriteTo:0:Args:restrictedToMinimumLevel"];
            Debug.WriteLine(logLevel);

            var cookie = Global.ServiceProviderRoot.GetRequiredService<BiliCookie>();

            Debug.WriteLine(JsonSerializer.Serialize(cookie, new JsonSerializerOptions { WriteIndented = true }));
            Assert.True(!string.IsNullOrWhiteSpace(cookie.UserId));
        }

        /// <summary>
        /// ���Ի�������Key�ķָ���
        /// </summary>
        [Fact]
        public void TestEnvKeyDelimiter()
        {
            Environment.SetEnvironmentVariable("Ray_BiliBiliCookie__UserId", "123");
            Program.CreateHost(null);

            string result = Global.ConfigurationRoot["BiliBiliCookie:UserId"];

            Assert.Equal("123", result);
        }

        [Fact]
        public void LoadPrefixConfigByEnvWithNoError()
        {
            Environment.SetEnvironmentVariable("Ray_BiliBiliCookie", "UserId: 123");
            Program.CreateHost(new string[] { });

            string result = Global.ConfigurationRoot["BiliBiliCookie"];

            Assert.Equal("UserId: 123", result);
            Environment.SetEnvironmentVariable("Ray_BiliBiliCookie", null);
        }

        [Fact]
        public void LoadPrefixConfigByEnvWhenValueIsNullWithNoError2()
        {
            Environment.SetEnvironmentVariable("Ray_BiliBiliCookie", null);
            Program.CreateHost(new string[] { });

            string result = Global.ConfigurationRoot["BiliBiliCookie"];

            Assert.Null(result);
        }

        [Fact]
        public void CoverConfigByEnvWithNoError()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
            Program.CreateHost(new string[] { });

            string result = Global.ConfigurationRoot["IsPrd"];

            Assert.Equal("True", result);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }

        /// <summary>
        /// Ϊ�����ֶ���ֵ
        /// </summary>
        [Fact]
        public void TestSetConfiguration()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            Program.CreateHost(new string[] { });

            var options = Global.ServiceProviderRoot.GetRequiredService<IOptionsMonitor<BiliBiliCookieOptions>>();
            Debug.WriteLine(options.CurrentValue.ToJsonStr());

            //�ֶ���ֵ
            //RayConfiguration.Root["BiliBiliCookie:UserId"] = "123456";
            //options.CurrentValue.UserId = "123456";

            Debug.WriteLine($"��Configuration��ȡ��{Global.ConfigurationRoot["BiliBiliCookie:UserId"]}");

            Debug.WriteLine($"����options��ȡ��{options.CurrentValue.ToJsonStr()}");

            var optionsNew = Global.ServiceProviderRoot.GetRequiredService<IOptionsMonitor<BiliBiliCookieOptions>>();
            Debug.WriteLine($"����options��ȡ��{optionsNew.CurrentValue.ToJsonStr()}");
        }

        /// <summary>
        /// Ϊ�����ֶ���ֵ
        /// </summary>
        [Fact]
        public void TestHostDefaults()
        {
            Debug.WriteLine(Environment.GetEnvironmentVariable(HostDefaults.EnvironmentKey));
        }

        [Fact]
        public void Test()
        {
            var s = "0123456";

            var s1 = s[..2];

            var s2 = s[4..];
        }
    }
}
