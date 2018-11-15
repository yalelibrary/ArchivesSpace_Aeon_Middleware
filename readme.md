# ASpace_Aeon_Middleware #
The ArchivesSpace Aeon Middleware replaces legacy software that was used to interact with the Archivist's Toolkit. It is still considered desirable to phase this software out in favor of direct import of information from the ArchivesSpace private and public APIs. With that said, however, this middleware currently powers the ArchivesSpace basic addon and the Aeon_2 web forms and is used to present information in a manner that has been reformatted to work with the legacy clients.

## Components ##

This software:
- Connects to the ArchivesSpace API via the ArchivesSpace .NET Client
- Makes available endpoints to retrieve information from ArchivesSpace
- Makes available endpoints to retrieve information from Orbis

### Endpoints - ATK ###

#### Get_AtkCache_Series.ashx ####

##### Request
- Url: `\qsearch_atkcache_holdings.ashx`
- Description: This endpoint searches ArchivesSpace for the provided query
- Parameters:

|Parameter Name|Value |Description                      |
|--------------|------|---------------------------------|
|repo          |String|Active repository code for aspace|
|type          |String|Either 'call_no_ms' or 'title'   |
|q             |String|Search query                     |

##### Special Notes
- Tabs and other whitespace characters in search are replaced with single spaces (" ")
- Values for "type" other than "call_no_ms" or "title" default to "call_no_ms" (including blank) *but* "type" isn't actually used - the query to ArchivesSpace is always the same
- Hyphen is removed from call number in search results

##### Field Translations
The tokens in the ArchivesSpace results field mapped to values in the response 
- **Call Number**: "Identifier"
- **Formatted Call Number**: Call number with the search values underlined/bolded
- **Location**: This is not used by the javascript or addon and is left blank. It was mapped in the cache to the empty "collection" field
- **Title**: "Title"
- **EmphTitle**: Title with search terms underlined/bolded
- **Author**: Left blank
- **EmphAuthor**: Left blank
- **Published**: Left blank
- **Search string**: The input string
- **ResourceID**: Resource ID (int from URI)
- **EadId**: Finding aid handle - taken from field ead_id, should possibly be replaced with full URI

##### Response
- Response Type: `text/plain`
```
call number|formatted call number|location|title|formatted title|Author|Formatted Author|Published|searchString|resourceID + "+" + resourceId + "+-+-"|eadId|callNumber
```
No results: 
```
```

Sample Data:
```
MS 193|<b><u>MS</u></b> <b><u>193</u></b>||Eliot (Jacob) Family Papers|Eliot (Jacob) Family Papers||||ms 193|2932+2932+-+-|mssa.ms.0193|MS 193
RU 193|RU <b><u>193</u></b>||Alumni Board, Yale University, Records|Alumni Board, Yale University, Records||||ms 193|2409+2409+-+-|mssa.ru.0193|RU 193
HM 193|HM <b><u>193</u></b>||Yale Lacrosse Records - scrapbooks [microform]|Yale Lacrosse Records - scrapbooks [microform]||||ms 193|5112+5112+-+-|mssa.hm.0193|HM 193
```

#### Get_AtkCache_Series.ashx ####

##### Request
- Url: `\get_atkcache_series.ashx`
- Description: This endpoint retrieves information about the top level (series) archival objects for a given resource.
- Parameters:

|Parameter Name|Value |Description                      |
|--------------|------|---------------------------------|
|Repo          |String|Active repository code for aspace|
|bib_id        |Int   |Resource ID                      |
|call_no       |String|Call number                      |

##### Response
- Response Type: `application/xml`
```xml
<rows>
	<row>
		<series_id><!--Series Archival Object ID--></series_id>
		<series_div><!--Series Division Name--></series_div>
		<series_title><!--Series Title--></series_title>
		<collection_title><!--Resource title--></collection_title>
		<ead_location><!--Finding Aid URI--></ead_location>
	</row>
</rows>
```
No results: 
```xml
<rows>
    <row>
        <bib_id>(not found)</bib_id>
    </row>
</rows>
```

Sample Data:
```xml
<rows>
    <row>
        <series_id>2021491</series_id>
        <series_div>Series Tape 1</series_div>
        <series_title>Un vétéran de Lam Djoulbe II, recorded at Ségou , Circa 1966, 1970-1979</series_title>
        <collection_title>Djibril Tamsir Niane audiorecordings documenting Guinean oral traditions</collection_title>
        <ead_location>http://hdl.handle.net/10079/fa/mssa.ms.1935</ead_location>
    </row>
</rows>
```

#### Get_AtkCache_Enums.ashx ####

##### Request
- Url: `\get_atkcache_enums.ashx`
- Description: This endpoint retrieves information for a given "holding record" - more specifically it accepts information about a series-level holding and then returns details about the top containers associated with that series.
- Parameters:

|Parameter Name|Value |Description                      |
|--------------|------|---------------------------------|
|Repo          |String|Active repository code for aspace|
|mfhd_id       |Int   |Resource ID                      |
|bib_id        |Int   |Resource ID (yes, actually)      |
|call_no       |String|Call number                      |
|series_id     |Int   |Archival Object ID - series level|


##### Response
- Response Type: `application/xml`
```xml
<rows>
    <row>
	    <item_id><!--Top Container ID--></item_id>
	    <enumeration><!--Top Container Enumeration--></enumeration>
	    <item_barcode><!--Top Container Barcode--></item_barcode>
	    <suppress_in_opac><!--Top Container Restricted--></suppress_in_opac>
	    <location><!--Top Container Location--></location>
	    <subLocation><!--Top Container Physical Description--></subLocation>
	    <callNumber><!--Resource call number--></callNumber>
    </row>
</rows>
```
No results: 
```xml
<rows>
</rows>
```

Sample Data:
```xml
<rows>
	<row>
		<item_id>198063</item_id>
		<enumeration>Box 4</enumeration>
		<item_barcode>39002104786109</item_barcode>
		<suppress_in_opac>Y</suppress_in_opac>
		<location>Library Shelving Facility [LSF]</location>
		<subLocation>audiotape (7 inch)</subLocation>
		<callNumber>MS 1935</callNumber>
	</row>
</rows>
```

#### list_atkcache_barcode_info.ashx ####

##### Request
- Url: `\list_atkcache_barcode_info.ashx`
- Description: This endpoint retrieves information for a barcode or item id (top container id). It returns an array of rows but should only ever have one item in that array.
- Parameters:

|Parameter Name|Value |Description                      |
|--------------|------|---------------------------------|
|Repo          |String|Active repository code for aspace|
|item_id       |Int   |Top container id                 |
|barcode       |String|Item barcode                     |

##### Special Notes
- I'm unsure why author and enumeration are blank or whether there would be problems re-adding them
- Hyphen is removed from call number in search results

##### Response
- Response Type: `application/xml`
```xml
<rows>
    <row>
        <mfhd_id><!--Resource ID--></mfhd_id>
        <call_no><!--Call Number--></call_no>
        <collection><!--Location--></collection>
        <author><!--Blank--></author>
        <title><!--Resource Title--></title>
        <enumeration><!--Blank--></enumeration>
    </row>
</rows>
```
No results: 
```xml
<rows>
</rows>
```

Sample Data:
```xml
<rows>
    <row>
        <mfhd_id>4876</mfhd_id>
        <call_no>MS 343</call_no>
        <collection>Library Shelving Facility [LSF]</collection>
        <author></author>
        <title>Othniel Charles Marsh papers</title>
        <enumeration></enumeration>
    </row>
</rows>
```



### Endpoints - Orbis ###


 
