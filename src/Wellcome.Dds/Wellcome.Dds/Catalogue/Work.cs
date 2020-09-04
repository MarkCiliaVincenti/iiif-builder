﻿namespace Wellcome.Dds.Catalogue
{
    public class Work : CatalogueEntity
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string[] AlternativeTitles { get; set; }
        public string Description { get; set; }
        public string PhysicalDescription { get; set; }
        public LabelledEntity WorkType { get; set; }
        public string Lettering { get; set; }
        public Contributor[] Contributors { get; set; }
        public Identifier[] Identifiers { get; set; }
        public Classification[] Subjects { get; set; }
        public Classification[] Genres { get; set; }
        public Location Thumbnail { get; set; }
        public Item[] Items { get; set; }
        public ProductionEvent[] Production { get; set; }
        public LabelledEntity Language { get; set; }
        public string Edition { get; set; }
        public Note[] Notes { get; set; }

        /// <summary>
        /// Optional extra: works that are matches on the same identifier (b number)
        /// </summary>
        public Work[] RelatedByIdentifier { get; set; }

        public override string ToString()
        {
            return $"{Id}: {Title}";
        }

    }
}
