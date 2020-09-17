﻿using ClangSharp.Pathogen;
using System;

namespace Biohazrd
{
    public sealed record TranslatedNormalField : TranslatedField
    {
        public TypeReference Type { get; init; }
        public bool IsBitField { get; init; }

        internal unsafe TranslatedNormalField(TranslationUnitParser parsingContext, TranslatedFile file, PathogenRecordField* field)
            : base(parsingContext, file, field)
        {
            if (field->Kind != PathogenRecordFieldKind.Normal)
            { throw new ArgumentException("The specified field must be a normal field.", nameof(field)); }

            Type = new ClangTypeReference(parsingContext, field->Type);
            IsBitField = field->IsBitField != 0;
        }

        public override string ToString()
            => base.ToString();
    }
}
