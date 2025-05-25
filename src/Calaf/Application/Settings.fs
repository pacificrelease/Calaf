namespace Calaf.Application

type DotNetXmlFilePattern = private DotNetXmlFilePattern of string

type TagCount = private TagCount of byte

type BumpSettings = {
    ProjectsSearchPattern: DotNetXmlFilePattern
    TagsToLoad: TagCount
}