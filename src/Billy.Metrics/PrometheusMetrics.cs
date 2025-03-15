using System;
using Prometheus.Client;
using Prometheus.Client.Abstractions;

namespace Billy.Metrics
{
    public static class PrometheusMetrics
    {
        public static IHistogram DatabaseRequestTime(string queryType) =>
            _databaseRequestTime.WithLabels(queryType);

        private static readonly Histogram _databaseRequestTime = Prometheus.Client.Metrics.CreateHistogram(
            name: "app_database_request_seconds",
            help: "Duration of database requests",
            labelNames: "query_type"
        );

        public static IHistogram ProjectionEventTime(string projection, string eventType) =>
            _projectionEventTime.WithLabels(projection, eventType);

        private static readonly Histogram _projectionEventTime = Prometheus.Client.Metrics.CreateHistogram(
            name: "app_projection_event_seconds",
            help: "Duration of processing an even by projection",
            labelNames: new[] { "projection", "event_type" }
        );


        public static IHistogram StorageRequestTime(string operationType, string storageName) =>
            _storageRequestTime.WithLabels(operationType, storageName);

        private static readonly Histogram _storageRequestTime = Prometheus.Client.Metrics.CreateHistogram(
            name: "app_storage_request_seconds",
            help: "Duration of storage requests",
            labelNames: new[] { "operation_type", "storage_name"}
        );

        public static IHistogram RecognitionAlgorithmRequestTime(string algorithmName) =>
            _recognitionAlgorithmRequestTime.WithLabels(algorithmName);

        private static readonly Histogram _recognitionAlgorithmRequestTime = Prometheus.Client.Metrics.CreateHistogram(
            name: "app_recognition_algorithm_request_seconds",
            help: "Duration of receipt recognition algorithm work",
            labelNames: "algorithm_name"
        );

        public static IHistogram OCRRequestTime(string providerName) =>
            _ocrRequestTime.WithLabels(providerName);

        private static readonly Histogram _ocrRequestTime = Prometheus.Client.Metrics.CreateHistogram(
            name: "app_ocr_request_seconds",
            help: "Duration of OCR requests",
            labelNames: "provider_name"
        );

        public static IGauge ApplicationHealthCheck(string healthyCheckType) =>
            _applicationHealthCheck.WithLabels(healthyCheckType);

        private static readonly Gauge _applicationHealthCheck = Prometheus.Client.Metrics.CreateGauge(
            name: "app_health_check",
            help: "Checking if application is healthy.",
            labelNames: new[] {"healthy_check_type"});

        public static IHistogram ConsumerProcessingTime(string messageType) =>
            _consumerProcessingTime.WithLabels(messageType);

        private static readonly Histogram _consumerProcessingTime = Prometheus.Client.Metrics.CreateHistogram(
            name: "app_consumers_processing_seconds",
            help: "Duration of messages processing",
            labelNames: "message_type"
        );
        
        public static IHistogram LogicProcessingTime(string context, string logicName) => 
            _logicProcessingTime.WithLabels(context, logicName);
        
        private static readonly Histogram _logicProcessingTime = Prometheus.Client.Metrics.CreateHistogram(
            name: "app_logic_processing_seconds",
            help: "Duration of logic processing",
            labelNames: new[] { "context", "logic_name"});
    }
}
