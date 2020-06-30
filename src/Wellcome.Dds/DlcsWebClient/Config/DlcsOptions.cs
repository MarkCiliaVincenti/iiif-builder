﻿namespace DlcsWebClient.Config
{
    public class DlcsOptions
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int CustomerDefaultSpace { get; set; }
        public string ApiEntryPoint { get; set; }
        public string ResourceEntryPoint { get; set; }
        public int BatchSize { get; set; } = 100;
        public bool PreventSynchronisation { get; set; } = false;
    }
}
