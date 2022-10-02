using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Splitwise.Models
{
  public enum CountryCode
  {
    CA,
    HU
  }

  public enum CurrencyCode
  {
    USD,
    ARS,
    AUD,
    EUR,
    BRL,
    CAD,
    CNY,
    DKK,
    GBP,
    INR,
    ILS,
    JPY,
    MXN,
    NZD,
    PHP,
    RUB,
    SGD,
    SEK,
    CHF,
    MYR,
    RON,
    ZAR,
    LKR,
    NAD,
    SAR,
    AED,
    PLN,
    HRK,
    PKR,
    TWD,
    VEF,
    HUF,
    CLP,
    BDT,
    CZK,
    COP,
    TRY,
    KRW,
    BOB,
    VND,
    NOK,
    EGP,
    HKD,
    THB,
    KES,
    IDR,
    ISK,
    BTC,
    UAH,
    MVR,
    OMR,
    YER,
    IRR,
    QAR,
    BHD,
    TZS,
    RSD,
    ETB,
    BGN,
    FJD,
    JMD,
    UYU,
    GTQ,
    NPR,
    PEN,
    DJF,
    LTL,
    MKW,
    KWD,
    CRC,
    DOP,
    NGN,
    JOD,
    MAD,
    RWF,
    UGX,
    AOA,
    XAF,
    XOF,
    CMG,
    ANG,
    ALL,
    PYG,
    KYD,
    KZT,
    BAM,
    AWG,
    BIF,
    MKD,
    XPF,
    GEL,
    TND,
    MZN,
    BYR,
    TTD,
    XCD,
    LBP,
    LAK,
    MOP,
    GHS,
    UZS,
    NIO,
    AZN,
    ZMW,
    SZL,
    BWP,
    MMK,
    CVE,
    MUR,
    SCR,
    KHR,
    CUP,
    CUC,
    STD,
    HNL,
    AMD,
    MDL,
    MNT,
    BYN,
    MGA,
    BBD,
    KMF,
    IQD,
    BZD,
    GYD,
    SRD,
    KGS,
    TJS,
    VUV,
    BTN,
    WST
  }

  public record class Picture(string Small, string Medium, string Large);

  public enum RegistrationStatus
  {
    Dummy,
    Invited,
    Confirmed
  }

  public record class Token(string AccessToken, TokenType TokenType);

  public enum TokenType
  {
    Bearer
  }

  public record class User(
    int Id,
    string FirstName,
    string LastName,
    Picture Picture,
    bool CustomPicture,
    string Email,
    RegistrationStatus RegistrationStatus,
    object ForceRefreshAt,
    string Locale,
    CountryCode CountryCode,
    string DateFormat,
    CurrencyCode DefaultCurrency,
    int DefaultGroupId,
    DateTime NotificationsRead,
    int NotificationsCount,
    Dictionary<string, bool> Notifications
  );
}
