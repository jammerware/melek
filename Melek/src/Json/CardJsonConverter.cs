﻿using System;
using Melek;
using Newtonsoft.Json.Linq;

namespace Melek.Json
{
    public class CardJsonConverter : JsonCreationConverter<ICard>
    {
        protected override Type GetType(Type objectType, JObject jObject)
        {
            if(jObject.Property("FlippedName") != null) {
                return typeof(FlipCard);
            }
            else if (jObject.Property("LeftCost") != null) {
                return typeof(SplitCard);
            }
            else if (jObject.Property("TransformedName") != null) {
                return typeof(TransformCard);
            }
            return typeof(Card);
        }
    }
}