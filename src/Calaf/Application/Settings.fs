namespace Calaf.Application

type DotNetXmlFilePattern = private DotNetXmlFilePattern of string

type TagQuantity = private TagQuantity of byte

type BumpSettings = {
    ProjectsSearchPattern: DotNetXmlFilePattern
    TagsToLoad: TagQuantity
}