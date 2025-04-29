// Decompiled with JetBrains decompiler
// Type: Microsoft.Synchronization.Services.Formatters.FormatterConstants
// Assembly: SiaqodbSyncProvider, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 45756F40-2044-4D99-BD9E-C1D39F89AA18
// Assembly location: \\Mac\Home\Downloads\SiaqodbSyncFW\bin\Siaqodb_5x\SiaqodbSyncProvider.dll

using Microsoft.Synchronization.ClientServices;
using System;
using System.Xml.Linq;

namespace Microsoft.Synchronization.Services.Formatters {
    internal static class FormatterConstants
  {
    public static XNamespace AtomNamespaceUri = XNamespace.Get("http://www.w3.org/2005/Atom");
    public static XNamespace XmlNamespace = XNamespace.Get("http://www.w3.org/2000/xmlns/");
    public static XNamespace SyncNamespace = XNamespace.Get("http://odata.org/sync/v1");
    public static XNamespace EdmxNamespace = XNamespace.Get("http://schemas.microsoft.com/ado/2007/06/edmx");
    public static XNamespace ODataMetadataNamespace = XNamespace.Get("http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
    public static XNamespace ODataDataNamespace = XNamespace.Get("http://schemas.microsoft.com/ado/2007/08/dataservices");
    public static XNamespace ODataSchemaNamespace = XNamespace.Get("http://schemas.microsoft.com/ado/2007/08/dataservices/schema");
    public static XNamespace AtomDeletedEntryNamespace = XNamespace.Get("http://purl.org/atompub/tombstones/1.0");
    public static XNamespace AtomXmlNamespace = XNamespace.Get("http://www.w3.org/2005/Atom");
    public static XNamespace JsonNamespace = XNamespace.Get("http://tempuri.org");
    public static DateTime JsonDateTimeStartTime = new DateTime(1970, 1, 1);
    public static long JsonNanoToMilliSecondsFactor = 10000;
    public static readonly Type DateTimeType = typeof (DateTime);
    public static readonly Type DateTimeOffsetType = typeof (DateTimeOffset);
    public static readonly Type TimeSpanType = typeof (TimeSpan);
    public static readonly Type ByteArrayType = typeof (byte[]);
    public static readonly Type BoolType = typeof (bool);
    public static readonly Type FloatType = typeof (float);
    public static readonly Type DecimalType = typeof (Decimal);
    public static readonly Type GuidType = typeof (Guid);
    public static readonly Type StringType = typeof (string);
    public static readonly Type CharType = typeof (char);
    public static readonly Type NullableType = typeof (Nullable<>);
    public static readonly Type SyncConflictResolutionType = typeof (SyncConflictResolution);
    public const string NullableTypeName = "Nullable`1";
    public const string MoreChangesAvailableText = "moreChangesAvailable";
    public const string ServerBlobText = "serverBlob";
    public const string PluralizeEntityNameFormat = "{0}s";
    public const string SyncConlflictElementName = "syncConflict";
    public const string SyncErrorElementName = "syncError";
    public const string ConflictEntryElementName = "conflictingChange";
    public const string ErrorEntryElementName = "changeInError";
    public const string ErrorDescriptionElementName = "errorDescription";
    public const string ErrorDescriptionElementNamePascalCasing = "ErrorDescription";
    public const string ConflictResolutionElementName = "conflictResolution";
    public const string IsDeletedElementName = "isDeleted";
    public const string IsConflictResolvedElementName = "isResolved";
    public const string TempIdElementName = "tempId";
    public const string EtagElementName = "etag";
    public const string EditUriElementName = "edituri";
    public const string SingleQuoteString = "'";
    public const string LeftBracketString = "(";
    public const string RightBracketString = ")";
    public const string ApplicationXmlContentType = "application/xml";
    public const string PropertiesElementName = "properties";
    public const string SyncNsPrefix = "sync";
    public const string EdmxNsPrefix = "edmx";
    public const string ODataMetadataNsPrefix = "m";
    public const string ODataDataNsPrefix = "d";
    public const string AtomDeletedEntryPrefix = "at";
    public const string AtomPubFeedElementName = "feed";
    public const string AtomPubEntryElementName = "entry";
    public const string AtomPubTitleElementName = "title";
    public const string AtomPubIdElementName = "id";
    public const string AtomPubContentElementName = "content";
    public const string AtomPubCategoryElementName = "category";
    public const string AtomPubUpdatedElementName = "updated";
    public const string AtomPubLinkElementName = "link";
    public const string AtomPubTermAttrName = "term";
    public const string AtomPubSchemaAttrName = "schema";
    public const string AtomPubRelAttrName = "rel";
    public const string AtomPubHrefAttrName = "href";
    public const string AtomPubXmlNsPrefix = "xmlns";
    public const string AtomPubTypeElementName = "type";
    public const string AtomPubIsNullElementName = "null";
    public const string AtomPubAuthorElementName = "author";
    public const string AtomPubNameElementName = "name";
    public const string AtomPubEditLinkAttributeName = "edit";
    public const string AtomDeletedEntryElementName = "deleted-entry";
    public const string AtomReferenceElementName = "ref";
    public const string AtomDateTimeOffsetLexicalRepresentation = "yyyy-MM-ddTHH:mm:ss.fffffffzzz";
    public const string AtomDateTimeLexicalRepresentation = "yyyy-MM-ddTHH:mm:ss.fffffff";
    public const string JsonDocumentElementName = "root";
    public const string JsonRootElementName = "d";
    public const string JsonTypeAttributeName = "type";
    public const string JsonSyncMetadataElementName = "__sync";
    public const string JsonSyncConflictElementName = "__syncConflict";
    public const string JsonSyncErrorElementName = "__syncError";
    public const string JsonSyncEntryMetadataElementName = "__metadata";
    public const string JsonSyncResultsElementName = "results";
    public const string JsonSyncEntryTypeElementName = "type";
    public const string JsonSyncEntryUriElementName = "uri";
    public const string JsonDateTimeFormat = "/Date({0})/";
    public const string JsonTimeFormat = "time'{0}'";
    public const string JsonDateTimeOffsetFormat = "datetimeoffset'{0}'";
    public const string JsonDateTimeOffsetLexicalRepresentation = "yyyy-MM-ddTHH:mm:ss.fffffffzzz";
    public const string JsonDateTimeLexicalRepresentation = "yyyy-MM-ddTHH:mm:ss.fffffff";
  }
}
