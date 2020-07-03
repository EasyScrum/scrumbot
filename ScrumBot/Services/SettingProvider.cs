using Microsoft.Extensions.Configuration;

namespace ScrumBot.Services
{
    public class SettingProvider
    {
        private readonly IConfiguration _configuration;

        public SettingProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private bool? _useTeams;
        public bool UseTeams
        {
            get
            {
                if (!_useTeams.HasValue)
                {
                    _useTeams = bool.TryParse(_configuration["UseTeams"], out var useTeams) && useTeams;
                }

                return _useTeams.Value;
            }
        }
    }
}
