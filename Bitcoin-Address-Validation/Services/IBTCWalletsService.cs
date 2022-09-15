namespace Bitcoin_Address_Validation.Services
{
    using Bitcoin_Address_Validation.Enums;
    using Bitcoin_Address_Validation.Models;
    public interface IBTCWalletsService
    {
        bool Validate(string address, Network? network);

        AddressInfo GetAddressInfo(string address);
    }
}
