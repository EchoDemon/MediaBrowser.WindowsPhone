﻿using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.System;

namespace MediaBrowser.Model
{
    public interface ISettingsService
    {
        UserDto LoggedInUser { get; }
        ConnectionDetails ConnectionDetails { get; set; }
        ServerConfiguration ServerConfiguration { get; set; }
        SystemInfo SystemStatus { get; set; }
    }
}
