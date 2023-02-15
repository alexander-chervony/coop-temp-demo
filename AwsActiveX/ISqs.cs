/*******************************************************
 * Copyright (C) 2018-2019 Alexander Chervony <alexander.chervony@gmail.com>
 * This file is part of CoOp project. It can not be copied and/or distributed
 * without the express permission of Alexander Chervony.
*******************************************************/

namespace AwsActiveX
{
    public interface ISqs
    {
        /// <summary>
        /// Sends message to specified queue.
        /// </summary>
        /// <param name="queueName">Queue name.</param>
        /// <param name="messageBody">An object serialized to json.</param>
        /// <returns>Http status code of the operation, 200 for success.</returns>
        int Send(string queueName, string messageBody);
        
        /// <summary>
        /// Deletes the message from queue. Should be called after successful processing of the subject message.
        /// </summary>
        /// <param name="queueName">Queue name.</param>
        /// <param name="receiptHandle">A handle of the message returned by Receive method.</param>
        /// <returns>Http status code of the operation, 200 for success.</returns>
        int Delete(string queueName, string receiptHandle);
        
        /// <summary>
        /// Receives available messages from queue.
        /// </summary>
        /// <param name="queueName">Queue name.</param>
        /// <param name="maxNumberOfMessages">A number from 1 to 10 messages to get at once. 10 is the Aws API limitation.</param>
        /// <returns>Json serialized object with status and list of messages, for example:
        /// {
        ///     "HttpStatusCode":200,
        ///     "ReceivedMessages":[
        ///         {"ReceiptHandle”:”string-handle-1”,”Body”:”json-serialized-object-1”},
        ///         {"ReceiptHandle”:”string-handle-2”,”Body”:”json-serialized-object-2”}]
        /// }
        /// </returns>
        string Receive(string queueName, int maxNumberOfMessages);
    }
}