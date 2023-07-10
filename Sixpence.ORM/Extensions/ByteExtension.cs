using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sixpence.ORM
{
    public static class ByteExtension
    {
        /// <summary>
        /// Bytes转流
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Stream ToStream(this byte[] bytes) => new MemoryStream(bytes);
    }
}
