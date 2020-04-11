namespace PublisherConfirms
{
    static class PublisherConfirms
    {
        public const int MessageCount = 50_000;

        public static void Main()
        {
            Individual.PublishMessagesIndividually();
            Batch.PublishMessagesInBatch();
            Async.HandlePublishConfirmsAsynchronously();
        }
    }
}