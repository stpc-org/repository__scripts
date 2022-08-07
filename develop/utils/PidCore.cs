using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.repository_scripts.develop.utils
{
    /**************************************************************************
		* 类 PIDCore
		* PID 核心
		* 
		* 增强
		* 比例系数:
		* 
		* 微分系数:
		* 
		* 积分系数:
		* [0] 积分抗饱和
		*       某一侧输出到达极限时不进行同侧积分
		* [1] 积分分离
		*       误差超过阈值时停止积分
		* [2] 变速积分
		*       误差越大积分作用越小, 否则越大
		* 
		**************************************************************************/
    class PidCore
    {
        // 分别是: 比例系数 (p), 积分系数 (i), 微分系数 (d)
        public double coefficient__proportion { get; set; } = 100;
        public double coefficient__integral { get; set; } = 25;
        public double coefficient__differential { get; set; } = 50;

        // 积分衰减速度
        // 1 表示标准衰减速度, <1 衰减更慢, >1 衰减更快
        public double coefficient__dynamic_integral_attenuation_ratio { get; set; } = 1;
        // 积分最高倍率, 值必须是一个正数, 相当于当误差趋近于 0 时, 增加积分的倍率
        public double coefficient__dynamic_integral_max_ratio { get; set; } = 1;

        // 上限 输出上限
        public double upper_limit__output { get; set; } = 10;
        // 下限, 输出下限
        public double lower_limit__output { get; set; } = -10;
        // 阈值 积分分离阈值 (也就是当误差绝对值大于这个值之后当次不再进行积分操作)
        public double threshold__integral_separation { get; set; } = 5;
        // 积分 最大值, 累计的积分的绝对值不可超过这个数字
        public double limit__integral { get; set; } = 20;

        // 标记 启用微分项
        public bool flag__enable_derivative_term { get; set; } = true;
        // 标记 启用积分项
        public bool flag__enable_integral_term { get; set; } = true;
        // 标记 启用积分抗饱和
        public bool flag__enable_integral_anti_windup { get; set; } = true;
        // 标记 启用积分分离 (当误差特别大的时候, 不进行积分操作, 避免干扰)
        public bool flag__enable_integral_separation { get; set; } = true;
        // 标记 启用动态积分 (变速积分)
        public bool flag__enable_dynamic_integral { get; set; } = true;
        // 标记 启用积分上限 (保证积分的绝对值不超过上限)
        public bool flag___enable_upper_limit_of_integral { get; set; } = true;

        // 调用计数
        public int count_invoke { get; private set; } = 0;

        public double integral { get; private set; } = 0.0;
        public double error__last { get; private set; } = 0.0;
        public double output__last { get; private set; } = 0.0;

        // 构造函数
        public PidCore()
        {

        }

        public PidCore
        (
            double _coefficient__proportion,
            double _coefficient__integral,
            double _coefficient__differential,
            double _coefficient__dynamic_integral_attenuation_ratio,
            double _coefficient__dynamic_integral_max_ratio,
            double _upper_limit__output,
            double _lower_limit__output,
            double _threshold__integral_separation,
            double _limit__integral,
            bool _flag__enable_derivative_term = true,
            bool _flag__enable_integral_term = true,
            bool _flag__enable_integral_anti_windup = true,
            bool _flag__enable_integral_separation = true,
            bool _flag__enable_dynamic_integral = true,
            bool _flag___enable_upper_limit_of_integral = true
        )
        {
            this.coefficient__proportion = _coefficient__proportion;
            this.coefficient__integral = _coefficient__integral;
            this.coefficient__differential = _coefficient__differential;
            this.coefficient__dynamic_integral_attenuation_ratio = _coefficient__dynamic_integral_attenuation_ratio;
            this.coefficient__dynamic_integral_max_ratio = _coefficient__dynamic_integral_max_ratio;
            this.upper_limit__output = _upper_limit__output;
            this.lower_limit__output = _lower_limit__output;
            this.threshold__integral_separation = _threshold__integral_separation;
            this.limit__integral = _limit__integral;
            this.flag__enable_derivative_term = _flag__enable_derivative_term;
            this.flag__enable_integral_term = _flag__enable_integral_term;
            this.flag__enable_integral_anti_windup = _flag__enable_integral_anti_windup;
            this.flag__enable_integral_separation = _flag__enable_integral_separation;
            this.flag__enable_dynamic_integral = _flag__enable_dynamic_integral;
            this.flag___enable_upper_limit_of_integral = _flag___enable_upper_limit_of_integral;
        }

        // 计算输出
        // [in] error 当前误差
        // [return]   当前误差对应的负反馈输出
        public double cal_output(double error)
        {
            double derivative = (this.flag__enable_derivative_term) ? (error - error__last) : 0.0;
            if (this.flag__enable_integral_term && (flag__enable_integral_separation ? (Math.Abs(error) < threshold__integral_separation) : true))
                if (flag__enable_integral_anti_windup ? ((output__last > upper_limit__output) ? (error > 0) : (output__last < lower_limit__output ? (error < 0) : true)) : true)
                    integral = Math.Max(-limit__integral, Math.Min(limit__integral, integral + (flag__enable_dynamic_integral ? (error / (coefficient__dynamic_integral_attenuation_ratio * Math.Abs(error) + 1 / coefficient__dynamic_integral_max_ratio)) : error)));
            ++count_invoke; error__last = error;
            output__last = -(coefficient__proportion * error + coefficient__integral * integral + coefficient__differential * derivative);
            return Math.Max(lower_limit__output, Math.Min(upper_limit__output, output__last));
        }
    }
}
