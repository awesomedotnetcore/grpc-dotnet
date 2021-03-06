﻿#region Copyright notice and license

// Copyright 2019 The gRPC Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.AspNetCore.Server;
using Grpc.AspNetCore.Server.Internal;

namespace Grpc.AspNetCore.FunctionalTests.Infrastructure
{
    internal static class MessageHelpers
    {
        private static readonly GrpcServiceOptions TestServiceOptions = new GrpcServiceOptions();

        public static T AssertReadMessage<T>(byte[] messageData) where T : IMessage, new()
        {
            var ms = new MemoryStream(messageData);

            return AssertReadMessageAsync<T>(ms).GetAwaiter().GetResult();
        }

        public static async Task<T> AssertReadMessageAsync<T>(Stream stream) where T : IMessage, new()
        {
            var pipeReader = new StreamPipeReader(stream);

            var messageData = await pipeReader.ReadSingleMessageAsync(TestServiceOptions);

            var message = new T();
            message.MergeFrom(messageData);

            return message;
        }

        public static async Task<T> AssertReadStreamMessageAsync<T>(PipeReader pipeReader) where T : IMessage, new()
        {
            var messageData = await pipeReader.ReadStreamMessageAsync(TestServiceOptions);

            if (messageData == null)
            {
                return default;
            }

            var message = new T();
            message.MergeFrom(messageData);

            return message;
        }

        public static void WriteMessage<T>(Stream stream, T message) where T : IMessage
        {
            var messageData = message.ToByteArray();

            var pipeWriter = new StreamPipeWriter(stream);

            PipeExtensions.WriteMessageAsync(pipeWriter, messageData, TestServiceOptions, flush: true).GetAwaiter().GetResult();
        }
    }
}
