﻿using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ray.BiliBiliTool.Agent;
using Ray.BiliBiliTool.Agent.BiliBiliAgent.Dtos;
using Ray.BiliBiliTool.Agent.BiliBiliAgent.Interfaces;
using Ray.BiliBiliTool.Config.Options;
using Ray.BiliBiliTool.DomainService.Interfaces;

namespace Ray.BiliBiliTool.DomainService
{
    /// <summary>
    /// 会员权益
    /// </summary>
    public class VipPrivilegeDomainService : IVipPrivilegeDomainService
    {
        private readonly ILogger<VipPrivilegeDomainService> _logger;
        private readonly IDailyTaskApi _dailyTaskApi;
        private readonly DailyTaskOptions _dailyTaskOptions;
        private readonly BiliCookie _biliBiliCookie;
        private readonly ReceiveVipPrivilegeOptions _receiveVipPrivilegeOptionsce;

        public VipPrivilegeDomainService(
            ILogger<VipPrivilegeDomainService> logger,
            IDailyTaskApi dailyTaskApi,
            BiliCookie biliBiliCookieOptions,
            IOptionsMonitor<DailyTaskOptions> dailyTaskOptions,
            IOptionsMonitor<ReceiveVipPrivilegeOptions> receiveVipPrivilegeOptionsce)
        {
            _logger = logger;
            _dailyTaskApi = dailyTaskApi;
            _dailyTaskOptions = dailyTaskOptions.CurrentValue;
            _receiveVipPrivilegeOptionsce = receiveVipPrivilegeOptionsce.CurrentValue;
            _biliBiliCookie = biliBiliCookieOptions;
        }

        /// <summary>
        /// 每月领取大会员福利（B币券、大会员权益）
        /// </summary>
        /// <param name="useInfo"></param>
        public async Task<bool> ReceiveVipPrivilege(UserInfo userInfo)
        {
            if (!_receiveVipPrivilegeOptionsce.IsEnable)
            {
                _logger.LogInformation("已配置为关闭，跳过");
                return false;
            }

            //大会员类型
            VipType vipType = userInfo.GetVipType();
            if (vipType != VipType.Annual)
            {
                _logger.LogInformation("普通会员和月度大会员每月不赠送B币券，不需要领取权益喽");
                return false;
            }

            /*
            int targetDay = _dailyTaskOptions.DayOfReceiveVipPrivilege == -1
                ? 1
                : _dailyTaskOptions.DayOfReceiveVipPrivilege;

            _logger.LogInformation("【目标领取日期】{targetDay}号", targetDay);
            _logger.LogInformation("【今天】{day}号", DateTime.Today.Day);

            if (DateTime.Today.Day != targetDay
                && DateTime.Today.Day != DateTime.Today.LastDayOfMonth().Day)
            {
                _logger.LogInformation("跳过");
                return false;
            }
            */

            var suc1 = await ReceiveVipPrivilege(VipPrivilegeType.BCoinCoupon);
            var suc2 = await ReceiveVipPrivilege(VipPrivilegeType.MembershipBenefits);

            if (suc1 | suc2) return true;
            return false;
        }

        #region private

        /// <summary>
        /// 领取大会员每月赠送福利
        /// </summary>
        /// <param name="type">1.大会员B币券；2.大会员福利</param>
        private async Task<bool> ReceiveVipPrivilege(VipPrivilegeType type)
        {
            var response = await _dailyTaskApi.ReceiveVipPrivilegeAsync((int)type, _biliBiliCookie.BiliJct);

            var name = GetPrivilegeName(type);
            _logger.LogInformation("【领取】{name}", name);

            if (response.Code == 0)
            {
                _logger.LogInformation("【结果】成功");
                return true;
            }
            else
            {
                _logger.LogInformation("【结果】失败");
                _logger.LogInformation("【原因】 {msg}", response.Message);
                return false;
            }
        }

        /// <summary>
        /// 获取权益名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetPrivilegeName(VipPrivilegeType type)
        {
            switch (type)
            {
                case VipPrivilegeType.BCoinCoupon:
                    return "年度大会员每月赠送的B币券";

                case VipPrivilegeType.MembershipBenefits:
                    return "大会员福利/权益";
            }

            return "";
        }

        #endregion private
    }
}
