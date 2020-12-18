using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;

namespace Unity.Tilemaps
{

    [Serializable]
    public class TilemapDataProvider
    {
        public virtual string FileExtensionName { get; }

        public virtual void Read(TilemapCreator creator, TilemapDataSettings options, Stream reader)
        {
            throw new NotImplementedException();
        }

        public virtual void Write(TilemapCreator creator, TilemapDataSettings options, Stream writer)
        {
            throw new NotImplementedException();
        }

    }


}
