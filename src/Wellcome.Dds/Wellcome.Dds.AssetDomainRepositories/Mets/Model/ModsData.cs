﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Utils;
using Wellcome.Dds.AssetDomain.Mets;

namespace Wellcome.Dds.AssetDomainRepositories.Mets.Model
{
    [Serializable]
    public class ModsData : IModsData
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Classification { get; set; }
        public string Language { get; set; }
        public string OriginDateDisplay { get; set; }
        public string OriginPlace { get; set; }
        public string OriginPublisher { get; set; }
        public string PhysicalDescription { get; set; }
        public string DisplayForm { get; set; }
        public string RecordIdentifier { get; set; }
        public IModsName[] Names { get; set; }

        public string Identifier { get; set; }
        public string AccessCondition { get; set; }
        public string DzLicenseCode { get; set; }
        public int PlayerOptions { get; set; }
        public string Usage { get; set; }
        public string Leader6 { get; set; }
        public string WellcomeIdentifier { get; set; }

        public string RepositoryName { get; set; }

        public int VolumeNumber { get; set; }
        public int CopyNumber { get; set; }
        
        public ModsData(XElement dmdSec)
        {
            var modsEl = dmdSec.GetSingleElementWithAttribute(XNames.MetsMdWrap, "MDTYPE", "MODS");
            var xmlData = modsEl.Element(XNames.MetsXmlData);
            Debug.Assert(xmlData != null, "xmlData != null");
            var modsDoc = new XDocument(xmlData.FirstNode);
            if (modsDoc.Root == null)
            {
                return;
            }

            // <mods:note type="noteType">value</mods:note>
            Leader6 = ExtractSingleModsNoteField(modsDoc, "leader6");
            WellcomeIdentifier = ExtractFirstModsNoteField(modsDoc, "wellcomeidentifier");
            var repoVals = GetDistinctValuesForModsNoteField(modsDoc, "repository name").ToList();
            if (repoVals.Any())
            {
                RepositoryName = string.Join("; ", repoVals);
            }

            Title = HtmlUtils.TextOnly(modsDoc.GetDesendantElementValue(XNames.ModsTitle));
            SubTitle = HtmlUtils.TextOnly(modsDoc.GetDesendantElementValue(XNames.ModsSubTitle));
            OriginPublisher = modsDoc.GetDesendantElementValue(XNames.ModsOriginPublisher);
            if (!OriginPublisher.HasText())
            {
                OriginPublisher = modsDoc.GetDesendantElementValue(XNames.ModsPublisher);
            }
            OriginPlace = modsDoc.GetDesendantElementValue(XNames.ModsPlaceTerm);
            OriginDateDisplay = GetCleanDisplayDate(modsDoc);
            Classification = modsDoc.GetDesendantElementValue(XNames.ModsClassification);
            Language = modsDoc.GetDesendantElementValue(XNames.ModsLanguageTerm);
            RecordIdentifier = modsDoc.GetDesendantElementValue(XNames.ModsRecordIdentifier);
            Identifier = modsDoc.GetDesendantElementValue(XNames.ModsIdentifier);
            PhysicalDescription = modsDoc.GetDesendantElementValue(XNames.ModsPhysicalDescription);
            DisplayForm = modsDoc.GetDesendantElementValue(XNames.ModsDisplayForm);
            Names = new IModsName[] { }; // TODO - not in xml at the moment

            var accessConditions = modsDoc.Root.Elements(XNames.ModsAccessCondition).ToList();

            var statusAccessConditionElement = accessConditions
                .SingleOrDefault(x => (string)x.Attribute("type") == "status");
            if (statusAccessConditionElement != null)
            {
                string accessConditionValue = statusAccessConditionElement.Value;
                if (Common.AccessCondition.IsValid(accessConditionValue))
                {
                    AccessCondition = accessConditionValue;
                }
            }
            if (!AccessCondition.HasText())
            {
                AccessCondition = Common.AccessCondition.Open;
            }

            var dzAccessConditionElement = accessConditions
                .SingleOrDefault(x => (string)x.Attribute("type") == "dz");
            if (dzAccessConditionElement != null)
            {
                DzLicenseCode = dzAccessConditionElement.Value;
            }

            var playerOptionsElement = accessConditions
                .SingleOrDefault(x => (string)x.Attribute("type") == "player");
            if (playerOptionsElement != null)
            {
                PlayerOptions = Convert.ToInt32(playerOptionsElement.Value);
            }

            var usageElement = accessConditions
                .SingleOrDefault(x => (string)x.Attribute("type") == "usage");
            if (usageElement != null)
            {
                // so where does this parse method go?
                // needs to replace bare license with link
                // then find its way into the attribution field
                // Usage = ParseUsage(usageElement.Value);

                Usage = usageElement.Value;
            }

            SetCopyAndVolumeNumbers(modsDoc, this);
        }

        private string ExtractSingleModsNoteField(XDocument modsDoc, string noteType)
        {
            var noteEl = modsDoc.Root
                .Descendants(XNames.ModsNote)
                .SingleOrDefault(x => (string)x.Attribute("type") == noteType);
            if (noteEl != null)
            {
                return noteEl.Value;
            }
            return null;
        }

        private string ExtractFirstModsNoteField(XDocument modsDoc, string noteType)
        {
            var noteEl = modsDoc.Root
                .Descendants(XNames.ModsNote)
                .FirstOrDefault(x => (string)x.Attribute("type") == noteType);
            if (noteEl != null)
            {
                return noteEl.Value;
            }
            return null;
        }

        private IEnumerable<string> GetDistinctValuesForModsNoteField(XDocument modsDoc, string noteType)
        {
            HashSet<string> found = new HashSet<string>();
            foreach (var val in modsDoc.Root
                .Descendants(XNames.ModsNote)
                .Where(x => (string)x.Attribute("type") == noteType)
                .Select(noteEl => noteEl.Value))
            {
                if (val.HasText() && !found.Contains(val))
                {
                    yield return val;
                    found.Add(val);
                }
            }
        }

        private string GetCleanDisplayDate(XDocument modsDoc)
        {
            string disp = null;
            int cutoffYear = DateTime.Now.AddYears(10).Year;
            try
            {
                foreach (var elementValue in modsDoc.GetDesendantElementValues(XNames.ModsDateIssued))
                {
                    if (elementValue.HasText())
                    {
                        if (elementValue.Length == 4)
                        {
                            int y;
                            if (Int32.TryParse(elementValue, out y))
                            {
                                if (y > cutoffYear)
                                    continue;
                            }
                        }
                        disp = elementValue;
                        break;
                    }
                }
            }
            catch
            {
                // log
            }
            return disp;
        }

        private void SetCopyAndVolumeNumbers(XDocument modsDoc, ModsData modsData)
        {
            var volumeNumber = modsDoc.GetDesendantElementValue(XNames.WtVolumeNumber);
            if (volumeNumber.HasText())
            {
                modsData.VolumeNumber = Convert.ToInt32(volumeNumber);
            }
            else
            {
                modsData.VolumeNumber = -1;
            }
            var copyNumber = modsDoc.GetDesendantElementValue(XNames.WtCopyNumber);
            if (copyNumber.HasText())
            {
                modsData.CopyNumber = Convert.ToInt32(copyNumber);
            }
            else
            {
                modsData.CopyNumber = 1;
            }
        }

        public IModsData GetDeepCopyForAccessControl()
        {
            // ModsName[] Names is a reference type but we don't mind our clone having the same pointer.
            return MemberwiseClone() as ModsData;
        }

        public string GetDisplayTitle()
        {
            return "" + Title + SubTitle;
        }
    }
}
