module App

open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props

type FileItem =
  | Directory of {| Id: int; Name: string; IsOpen: bool; Children: FileItem list |}
  | File of {| Id: int; Name: string |}

type State = { Files: FileItem list }

type Msg =
  | ToggleDirectory of int

let init() = {
  Files = [
    Directory {| Id = 1; Name = "Documents"; IsOpen = false ; Children = [
        File {| Id = 2; Name = "report.pdf" |};
        File {| Id = 3; Name = "image.png" |}
        Directory {| Id = 4; Name = "Programs"; IsOpen = false; Children = [ File {| Id = 5; Name = "word.exe" |} ] |}
      ]
    |}
  ]
}

let rec toggleDirectoryOpened id = function
  | File file -> File file
  | Directory directory when directory.Id = id ->
      Directory {| directory with IsOpen = not directory.IsOpen |}
  | Directory directory ->
      Directory {| directory with Children = List.map (toggleDirectoryOpened id) directory.Children |}

let update (msg: Msg) (state: State) =
  match msg with
  | ToggleDirectory id -> { state with Files = List.map (toggleDirectoryOpened id) state.Files }

let fileIcon = i [ Class "fa fa-file" ] [ ]
let openFolderIcon = i [ Class "fa fa-folder-open" ] [ ]
let closedFolderIcon = i [ Class "fa fa-folder" ] [ ]

let rec renderFile dispatch = function
  | File file ->
      AnimatedTree.animatedTree [
        AnimatedTree.Key file.Id
        AnimatedTree.Icon fileIcon
        AnimatedTree.Content (str file.Name)
      ]

  | Directory directory ->
      AnimatedTree.animatedTree [
        AnimatedTree.Key directory.Id
        AnimatedTree.Icon (if directory.IsOpen then openFolderIcon else closedFolderIcon)
        AnimatedTree.Content (str directory.Name)
        AnimatedTree.IsOpen directory.IsOpen
        AnimatedTree.OnToggled (fun _ -> dispatch (ToggleDirectory directory.Id))
        AnimatedTree.Children [ for file in directory.Children -> renderFile dispatch file  ]
      ]

let render (state: State) (dispatch: Msg -> unit) =
  div [ Style [ Padding 20; Color "white"; Fill "white"; Width "300px" ] ] [
    for file in state.Files -> renderFile dispatch file
  ]

Program.mkSimple init update render
|> Program.withReactSynchronous "elmish-app"
|> Program.withConsoleTrace
|> Program.run