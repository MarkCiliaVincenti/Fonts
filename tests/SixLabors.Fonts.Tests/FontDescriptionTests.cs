// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Globalization;
using SixLabors.Fonts.WellKnownIds;
using Xunit;

namespace SixLabors.Fonts.Tests
{
    public class FontDescriptionTests
    {
        [Fact]
        public void LoadFontDescription()
        {
            var writer = new BigEndianBinaryWriter();
            writer.WriteTrueTypeFileHeader(1, 0, 0, 0);
            writer.WriteTableHeader("name", 0, 28, 999);
            writer.WriteNameTable(
                new Dictionary<KnownNameIds, string>
                {
                    { KnownNameIds.FullFontName, "name" },
                    { KnownNameIds.FontSubfamilyName, "sub" },
                    { KnownNameIds.FontFamilyName, "fam" }
                });

            var description = FontDescription.LoadDescription(writer.GetStream());
            Assert.Equal("name", description.FontNameInvariantCulture);
            Assert.Equal("sub", description.FontSubFamilyNameInvariantCulture);
            Assert.Equal("fam", description.FontFamilyInvariantCulture);
        }

        [Fact]
        public void LoadFontDescription_CultureNamePriority_FirstWindows()
        {
            var usCulture = new CultureInfo(0x0409);
            var c1 = new CultureInfo(1034); // spanish - international
            var c2 = new CultureInfo(3082); // spanish - traditional

            var writer = new BigEndianBinaryWriter();
            writer.WriteTrueTypeFileHeader(1, 0, 0, 0);
            writer.WriteTableHeader("name", 0, 28, 999);
            writer.WriteNameTable(
                (KnownNameIds.FullFontName, "name_c1", c1),
                (KnownNameIds.FontSubfamilyName, "sub_c1", c1),
                (KnownNameIds.FontFamilyName, "fam_c1", c1),
                (KnownNameIds.FullFontName, "name_c2", c2),
                (KnownNameIds.FontSubfamilyName, "sub_c2", c2),
                (KnownNameIds.FontFamilyName, "fam_c2", c2));

            var description = FontDescription.LoadDescription(writer.GetStream());

            // unknown culture should prioritise US, but missing so will return first
            Assert.Equal("name_c1", description.FontNameInvariantCulture);
            Assert.Equal("sub_c1", description.FontSubFamilyNameInvariantCulture);
            Assert.Equal("fam_c1", description.FontFamilyInvariantCulture);
        }

        [Fact]
        public void LoadFontDescription_CultureNamePriority_US()
        {
            var usCulture = new CultureInfo(0x0409);
            var c1 = new CultureInfo(1034); // spanish - international
            var c2 = new CultureInfo(3082); // spanish - traditional

            var writer = new BigEndianBinaryWriter();
            writer.WriteTrueTypeFileHeader(1, 0, 0, 0);
            writer.WriteTableHeader("name", 0, 28, 999);
            writer.WriteNameTable(
                (KnownNameIds.FullFontName, "name_c1", c1),
                (KnownNameIds.FontSubfamilyName, "sub_c1", c1),
                (KnownNameIds.FontFamilyName, "fam_c1", c1),
                (KnownNameIds.FullFontName, "name_c2", c2),
                (KnownNameIds.FontSubfamilyName, "sub_c2", c2),
                (KnownNameIds.FontFamilyName, "fam_c2", c2),
                (KnownNameIds.FullFontName, "name_us", usCulture),
                (KnownNameIds.FontSubfamilyName, "sub_us", usCulture),
                (KnownNameIds.FontFamilyName, "fam_us", usCulture));

            var description = FontDescription.LoadDescription(writer.GetStream());

            // unknown culture should prioritise US, but missing so will return first
            Assert.Equal("name_us", description.FontNameInvariantCulture);
            Assert.Equal("sub_us", description.FontSubFamilyNameInvariantCulture);
            Assert.Equal("fam_us", description.FontFamilyInvariantCulture);
        }

        [Fact]
        public void LoadFontDescription_CultureNamePriority_ExactMatch()
        {
            var usCulture = new CultureInfo(0x0409);
            var c1 = new CultureInfo(1034); // spanish - international
            var c2 = new CultureInfo(3082); // spanish - traditional

            var writer = new BigEndianBinaryWriter();
            writer.WriteTrueTypeFileHeader(1, 0, 0, 0);
            writer.WriteTableHeader("name", 0, 28, 999);
            writer.WriteNameTable(
                (KnownNameIds.FullFontName, "name_c1", c1),
                (KnownNameIds.FontSubfamilyName, "sub_c1", c1),
                (KnownNameIds.FontFamilyName, "fam_c1", c1),
                (KnownNameIds.FullFontName, "name_c2", c2),
                (KnownNameIds.FontSubfamilyName, "sub_c2", c2),
                (KnownNameIds.FontFamilyName, "fam_c2", c2),
                (KnownNameIds.FullFontName, "name_us", usCulture),
                (KnownNameIds.FontSubfamilyName, "sub_us", usCulture),
                (KnownNameIds.FontFamilyName, "fam_us", usCulture));

            var description = FontDescription.LoadDescription(writer.GetStream());

            // unknown culture should prioritise US, but missing so will return first
            Assert.Equal("name_c2", description.FontName(c2));
            Assert.Equal("sub_c2", description.FontSubFamilyName(c2));
            Assert.Equal("fam_c2", description.FontFamily(c2));
        }

        [Fact]
        public void CanLoadFontCollectionDescriptionsFromPath()
        {
            FontDescription[] descriptions = FontDescription.LoadFontCollectionDescriptions(TestFonts.SimpleTrueTypeCollection);
            Assert.NotNull(descriptions);
            Assert.Equal(2, descriptions.Length);

            FontDescription description = descriptions[0];
            Assert.Equal("SixLaborsSampleAB", description.FontFamilyInvariantCulture);
            Assert.Equal("SixLaborsSampleAB regular", description.FontNameInvariantCulture);
            Assert.Equal("Regular", description.FontSubFamilyNameInvariantCulture);
            Assert.Equal(FontStyle.Regular, description.Style);

            description = descriptions[1];
            Assert.Equal("Open Sans", description.FontFamilyInvariantCulture);
            Assert.Equal("Open Sans", description.FontNameInvariantCulture);
            Assert.Equal("Regular", description.FontSubFamilyNameInvariantCulture);
            Assert.Equal(FontStyle.Regular, description.Style);
        }

        [Fact]
        public void LoadFontDescription_GetNameById()
        {
            var c1 = new CultureInfo(1034); // spanish - international
            var c2 = new CultureInfo(3082); // spanish - traditional

            var writer = new BigEndianBinaryWriter();
            writer.WriteTrueTypeFileHeader(1, 0, 0, 0);
            writer.WriteTableHeader("name", 0, 28, 999);
            writer.WriteNameTable(
                (KnownNameIds.FullFontName, "name_c1", c1),
                (KnownNameIds.FontSubfamilyName, "sub_c1", c1),
                (KnownNameIds.FontFamilyName, "fam_c1", c1),
                (KnownNameIds.FullFontName, "name_c2", c2),
                (KnownNameIds.FontSubfamilyName, "sub_c2", c2),
                (KnownNameIds.FontFamilyName, "fam_c2", c2));

            var description = FontDescription.LoadDescription(writer.GetStream());

            Assert.Equal("name_c1", description.GetNameById(c1, KnownNameIds.FullFontName));
            Assert.Equal("sub_c1", description.GetNameById(c1, KnownNameIds.FontSubfamilyName));
            Assert.Equal("fam_c1", description.GetNameById(c1, KnownNameIds.FontFamilyName));

            Assert.Equal("name_c2", description.GetNameById(c2, KnownNameIds.FullFontName));
            Assert.Equal("sub_c2", description.GetNameById(c2, KnownNameIds.FontSubfamilyName));
            Assert.Equal("fam_c2", description.GetNameById(c2, KnownNameIds.FontFamilyName));
        }

        [Fact]
        public void ThrowsCorrectExceptionWhenDecodingCollectionFromNonTTCFiles()
            => Assert.Throws<InvalidFontTableException>(() => FontDescription.LoadFontCollectionDescriptions(TestFonts.TwemojiMozillaFile));
    }
}
