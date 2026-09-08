# AGENTS.md — Modbus TCP 协议栈手写周

> 本文件是 AI 编程代理在本周工作的强制指引。本周的根本目标是**人学会协议**，不是代码产出速度。任何与该目标冲突的"效率"行为都违反本文件。

## 根本纪律：AI 只当教练，不写代码

- 所有实现代码由人亲手逐行编写。AI 禁止输出实现代码、重构代码、"顺手优化"片段——即使人主动索要，先反问"你自己打算怎么写"
- AI 的合法输出只有三种：协议概念解释、对人写的代码的评审意见（指出问题，不给改写）、测试用例建议
- 人写的每一行都要能回答"为什么这么写"——答不上来的行不允许提交。这是面试可辩护性标准，不是洁癖
- 评审按模块攒批进行：一个模块写完再送审，禁止逐函数询问
- Modbus 官方规范（modbus.org 免费公开）能查到的事实性问题——帧格式、功能码定义、异常码表——先查规范再问 AI

## 项目概述

手写工业通信协议库（南向协议族），独立公开仓库 `Sakuno.Communications`，命名沿用个人仓库既有约定（`Sakuno.Communications.Modbus.Tcp` 形态），零第三方依赖（不用 HslCommunication / NModbus）。Modbus TCP 是第一个交付物；架构按多协议预留，RTU 为既定后续扩展。

落地路径：成为 WaferSight 的第三个 `IDeviceClient` 实现，替换商业库；库稳定后发布 NuGet 包，WaferSight 以包引用接入，保持其"克隆即可运行"。

互操作验证：对接 WaferSight 现有 VirtualPlc（HslCommunication 实现的 Modbus 服务器）——自家手写客户端打第三方服务器，构成真实协议互操作；必要时 Wireshark 抓包做字节级比对。

## 技术栈（锁定）

- .NET 10 / C# 14，全链路 nullable
- 仅 BCL：`System.Net.Sockets`、`System.Buffers.Binary`（BinaryPrimitives）、`System.Memory`（Span/Memory）
- xUnit v3 + FluentAssertions
- 禁止引用任何第三方包

## 范围（锁定，未经要求不得扩大）

实现：

- MBAP 帧封装与解析（事务 ID / 协议 ID 恒 0 / 长度 / 单元 ID）
- 功能码 0x03（读保持寄存器）、0x06（写单寄存器）、0x10（写多寄存器）
- 异常响应解析（异常码 01–04）
- 超时与 CancellationToken 全链路传递
- 32 位数据字序：ABCD / BADC / CDAB / DCBA（对应 WaferSight 点位的 DataFormat）
- TCP 长连接管理与连接状态事件；**重连编排不在本库**——留给上层（WaferSight Application 层既有状态机）

明确不做（本周）：

- RTU / ASCII（串口）、功能码 0x01/0x02/0x04/0x05、加密——留扩展点，不实现
- 同一连接同一时间只允许一个未决请求（Modbus TCP 常见简化），文档注明该取舍

路线图（写入 README，但不进本周排期）：

- **Modbus RTU**：既定下一站。传输层换串口（CRC16 + 3.5 字符定帧），PDU 原样复用——架构约束已为此铺路
- **OPC UA**：只到"能聊架构"为止（地址空间 / NodeId / Subscription / MonitoredItem / Session），不手写实现；其协议栈体量是 Modbus 的数十倍，且业界通行做法是使用 OPC Foundation 参考栈而非自研
- **SECS/GEM**：北向协议，不进本周排期。若未来实现，编解码层可以 `Sakuno.Communications.Secs` 形态加入本族；GEM 语义映射（报警 → Collection Event）属业务侧，留在 WaferSight

边界：本库返回原始寄存器/字节；工程量换算（Scale/Offset）仍属 WaferSight 领域层，不下移。

## 架构约束

- **PDU 层与传输层分离，按包物理隔离**：
- `Sakuno.Communications.Modbus`（核心包）：PDU 编解码、异常码、字序工具——零传输依赖
- `Sakuno.Communications.Modbus.Tcp`：MBAP 帧、TCP 长连接、请求/响应编排，依赖核心包
- RTU 落地时新增 `Sakuno.Communications.Modbus.Rtu`（串口 + CRC16 + 3.5 字符定帧），依赖同一核心包，PDU 原样复用
- 多协议预留但不预建：本周只存在上述两个项目，禁止为 RTU / OPC UA 提前写任何抽象层
- 字节序显式化：一律 `BinaryPrimitives`，禁止 `BitConverter`（平台字节序依赖）
- 热路径零分配风格：Span / Memory / stackalloc，禁止用 LINQ 拼接字节数组
- 公共 API 写中文 XML 契约注释（用途、语义、线程归属）

## 验收标准

1. 对 VirtualPlc 读出全部点位，读数与现有 ModbusDeviceClient 一致
2. 经 0x10 写入配方目标值，VirtualPlc 侧寄存器确实变化
3. 帧级单元测试：已知输入 → 断言精确字节序列（含四种字序组合）
4. 异常码、超时各有测试
5. Wireshark 抓包逐字节能被人肉解释
6. 接入 WaferSight：本地 nupkg 包引用，配置切换第三实现，采集/报警/配方/断线全功能回归

## 五日计划

- D1–D2：精读规范 MBAP 与 0x03 章节 → MBAP 编解码 + 0x03 链路打通（对 VirtualPlc 读出真实数据）
- D3：0x06 / 0x10 写入链路（配方下发走自家协议栈）
- D4：异常码 + 超时 + 四种字序
- D5：打包本地 nupkg → 接入 WaferSight 作第三实现 + 全量回归 + 更新两边 README 与简历条目
- 允许滑动，不允许跳步：读不通不许写，写不通不许接

## 注释与文档纪律

沿用 WaferSight 仓库同名章节：注释现在时、里程碑编号不入代码与 commit message、TODO 清零、六个月自检。

语言纪律：标识符（命名空间 / 类型 / 成员）一律英文，禁止拼音；协议术语保留英文原词（MBAP、PDU、RTU、CRC）；注释、XML 文档、README 用中文——目标读者是中文工业团队，与 WaferSight 仓库保持一致。commit message 用英文——本仓库独立公开且既有提交历史为英文。

## 常用命令

```powershell
dotnet build
dotnet test
dotnet pack
```