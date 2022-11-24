using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi;

public enum Controller : byte
{
    BankSelectMsb = 0,
    ModulationWheelMsb = 1,
    BreathMsb = 2,
    // 3 Undefined
    FootMsb = 4,
    PortamentoTimeMsb = 5,
    DataEntryMsb = 6,
    ChannelVolumeMsb = 7,
    BalanceMsb = 8,
    // 9 Undefined
    PanMsb = 10,
    ExpressionMsb = 11,
    EffectControl1Msb = 12,
    EffectControl2Msb = 13,
    // 14-15 Undefined
    GeneralPurpose1Msb = 16,
    GeneralPurpose2Msb = 17,
    GeneralPurpose3Msb = 18,
    GeneralPurpose4Msb = 19,
    // 20-31 Undefined

    // 32-63 LSB for 0-31
    BankSelectLsb = 32,
    ModulationWheelLsb = 33,
    BreathLsb = 34,
    // 35 Undefined
    FootLsb = 36,
    PortamentoTimeLsb = 37,
    DataEntryLsb = 38,
    ChannelVolumeLsb = 39,
    BalanceLsb = 40,
    // 41 Undefined
    PanLsb = 42,
    ExpressionLsb = 43,
    EffectControl1Lsb = 44,
    EffectControl2Lsb = 45,
    // 16-47 Undefined
    GeneralPurpose1Lsb = 48,
    GeneralPurpose2Lsb = 49,
    GeneralPurpose3Lsb = 50,
    GeneralPurpose4Lsb = 51,
    // 52-63 Undefined

    DamperPedal = 64,
    PortamentoOnOff = 65,
    Sostenuto = 66,
    SoftPedal = 67,
    LegatoFootswitch = 68,
    Hold2 = 69,
    Sound1_Variation = 70,
    Sound2_Timber = 71,
    Sound3_ReleaseTime = 72,
    Sound4_AttackTime = 73,
    Sound5_Brightness = 74,
    Sound6 = 75,
    Sound7 = 76,
    Sound8 = 77,
    Sound9 = 78,
    Sound10 = 79,
    GeneralPurpose5 = 80,
    GeneralPurpose6 = 81,
    GeneralPurpose7 = 82,
    GeneralPurpose8 = 83,
    PortamentoControl = 84,
    // 85-90 Undefined
    Effects1Depth = 91,
    Effects2Depth = 92,
    Effects3Depth = 93,
    Effects4Depth = 94,
    Effects5Depth = 95,
    DataIncrement = 96,
    DataDecrement = 97,
    NonRegisteredParameterNumberLsb = 98,
    NonRegisteredParameterNumberMsb = 99,
    RegisteredParameterNumberLsb = 100,
    RegisteredParameterNumberMsb = 101,
    // 102-119 Undefined
    // 120+ Channel Mode
}

public static class ControllerEnum
{
    public const byte ExclusiveMaxValue = 120;

    public static bool IsValidByte(byte value) => value < ExclusiveMaxValue;
    public static Controller? FromByte(byte value) => IsValidByte(value) ? (Controller)value : null;
    public static byte ToByte(this Controller value) => (byte)value;

    public static Controller? GetLsbIfMsb(this Controller value)
        => (int)value is >= 0 and < 32 ? value + (byte)32 : null;

    public static bool IsValid(this Controller value) => (byte)value < ExclusiveMaxValue;
}
