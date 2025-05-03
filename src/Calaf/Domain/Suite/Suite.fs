module internal Calaf.Domain.Suite

open Calaf.Domain.DomainTypes

let create (projects: Project[]) : Suite =
    { Version  = projects |> Project.chooseCalendarVersions |> Version.tryMax
      Projects = projects }

