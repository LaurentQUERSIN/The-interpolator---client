#region -- License Terms --
//
// MessagePack for CLI
//
// Copyright (C) 2010-2012 FUJIWARA, Yusuke
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
#endregion -- License Terms --

#if UNITY_IOS

using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

namespace MsgPack.Serialization.DefaultSerializers
{
    internal class EnumMessagePackSerializer : MessagePackSerializer
    {

        public EnumMessagePackSerializer(Type type, PackerCompatibilityOptions packerCompatibilityOptions)
            : base(type, packerCompatibilityOptions)
        {
            if (!type.GetIsEnum())
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "Type '{0}' is not enum.", type));
            }
        }

        protected internal sealed override void PackToCore(Packer packer, object value)
        {
            packer.PackString(value.ToString());
        }

        protected internal sealed override object UnpackFromCore(Unpacker unpacker)
        {
            object value;
            try
            {
                value = Enum.Parse(TargetType, unpacker.LastReadData.AsString(), false);
            }
            catch (ArgumentException)
            {
                throw new SerializationException(String.Format(CultureInfo.CurrentCulture, "'{0}' is not valid for enum type '{1}'.", unpacker.LastReadData.AsString(), TargetType));
            }
            return value;
        }
    }
}

#else
using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

namespace MsgPack.Serialization.DefaultSerializers
{
	internal static class EnumMessagePackSerializer
	{
		public static readonly MethodInfo Unmarshal1Method = typeof( EnumMessagePackSerializer ).GetMethod( "Unmarshal" );

		public static T Unmarshal<T>( Unpacker unpacker )
			where T : struct
		{
			T value;
			try
			{
				value = ( T )Enum.Parse( typeof( T ), unpacker.LastReadData.AsString(), false );
			}
			catch( ArgumentException )
			{
				throw new SerializationException( String.Format( CultureInfo.CurrentCulture, "'{0}' is not valid for enum type '{1}'.", unpacker.LastReadData.AsString(), typeof( T ) ) );
			}
			return value;
		}
	}
}
#endif