namespace GetSubscriptionTable_1
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Net.Messages;

    /// <summary>
    /// Represents a data source.
    /// See: https://aka.dataminer.services/gqi-external-data-source for a complete example.
    /// </summary>
    [GQIMetaData(Name = "GetSubscriptionTable")]
    public sealed class GetSubscriptionTable : IGQIDataSource, IGQIOnInit, IGQIInputArguments
    {
        private readonly GQIStringArgument nameFilter = new GQIStringArgument("Name Filter") { IsRequired = false };

        private string _nameFilter;
        private GQIDMS _dms;

        public OnInitOutputArgs OnInit(OnInitInputArgs args)
        {
            _dms = args.DMS;
            return new OnInitOutputArgs();
        }

        public GQIArgument[] GetInputArguments()
        {
            return new GQIArgument[] { nameFilter };
        }

        public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
        {
            _nameFilter = args.GetArgumentValue(nameFilter);
            return new OnArgumentsProcessedOutputArgs();
        }

        public GQIColumn[] GetColumns()
        {
            return new GQIColumn[]
            {
                    new GQIStringColumn("Subscription Name"),
                    new GQIDateTimeColumn("Connection Time"),
            };
        }

        public GQIPage GetNextPage(GetNextPageInputArgs args)
        {
            var rows = GetSubscriptions().ToArray();
            return new GQIPage(rows)
            {
                HasNextPage = false,
            };
        }

        private IEnumerable<GQIRow> GetSubscriptions()
        {
            var rows = new List<GQIRow>();

            var clientInfoMessagerResponse = _dms.SendMessages(new GetInfoMessage(InfoType.ClientList));
            var responses = clientInfoMessagerResponse?.OfType<LoginInfoResponseMessage>().ToList();
            int i = 0;
            foreach (var response in responses)
            {
                if (!response.FriendlyName.Contains(_nameFilter))
                {
                    continue;
                }

                var cells = new[]
                {
                    new GQICell { Value = Convert.ToString(response.FriendlyName )}, // Channel Id
                    new GQICell { Value = response.ConnectTime.ToUniversalTime() },
                };

                var row = new GQIRow(i.ToString(), cells);
                i++;

                rows.Add(row);
            }

            return rows;
        }
    }
}
