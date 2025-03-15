using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Billy.Metrics
{
    public delegate void ReportDuration(double seconds);

    public class TimerScope : IDisposable
    {
        private readonly ReportDuration _reportDuration;
        private Stopwatch _stopwatch;

        public TimerScope(ReportDuration reportDuration)
        {
            _reportDuration = reportDuration;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _reportDuration(_stopwatch.ElapsedTicks / (double)Stopwatch.Frequency);
        }
    }

    public class DbQueryTimer : TimerScope
    {
        public DbQueryTimer(string queryType)
            : base(seconds => PrometheusMetrics.DatabaseRequestTime(queryType).Observe(seconds)) { }
    }
    
    public class ProjectionEventTimer : TimerScope
    {
        public ProjectionEventTimer(string projection, object @event)
            : base(seconds => PrometheusMetrics.ProjectionEventTime(projection, @event?.GetType()?.Name ?? "(unknown)").Observe(seconds)) { }

        public ProjectionEventTimer(string projection, string eventType)
            : base(seconds => PrometheusMetrics.ProjectionEventTime(projection, eventType).Observe(seconds)) { }
    }

    public class OcrTimer : TimerScope
    {
        public OcrTimer(string providerName) : base(seconds => PrometheusMetrics.OCRRequestTime(providerName).Observe(seconds)) {}
    }

    public class StorageAccessTimer : TimerScope
    {
        public StorageAccessTimer(string operationType, string storageName) : base(seconds => PrometheusMetrics.StorageRequestTime(operationType, storageName).Observe(seconds)) { }
    }

    public class RecognitionAlgorithmTimer : TimerScope
    {
        public RecognitionAlgorithmTimer(string algorithmName) : base(seconds => PrometheusMetrics.RecognitionAlgorithmRequestTime(algorithmName).Observe(seconds)) { }
    }

    public class MessageProcessingTimer : TimerScope
    {
        public MessageProcessingTimer(string messageType) : base(seconds => PrometheusMetrics.ConsumerProcessingTime(messageType).Observe(seconds)) { }
    }

    public class LogicProcessingTimer : TimerScope
    {
        public LogicProcessingTimer(string context, string logicName) : base(seconds => PrometheusMetrics.LogicProcessingTime(context,logicName).Observe(seconds)) { }
    }
}
