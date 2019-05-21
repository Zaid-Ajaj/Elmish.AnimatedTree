[<RequireQualifiedAccess>]
module AnimatedTree

open Fable.React
open Fable.Core
open Fable.Core.JsInterop
open Fable.React.Props

type private Keys =
  | [<CompiledName "content">] Content
  | [<CompiledName "type">] Type
  | [<CompiledName "open">] IsOpen
  | [<CompiledName "canHide">] CanHide
  | [<CompiledName "visible">] Visible
  | [<CompiledName "onClick">] OnClick
  | [<CompiledName "children">] Children
  | [<CompiledName "className">] Class
  | [<CompiledName "style">] Style
  | [<CompiledName "id">] Id
  | [<CompiledName "toggled">] Toggled

type IAnimatedTreeProp = interface end

let Content (content: ReactElement) = (Keys.Content, content) |> unbox<IAnimatedTreeProp>
let Icon (value: ReactElement) = (Keys.Type, value) |> unbox<IAnimatedTreeProp>
let IsOpen (condition: bool) = (Keys.IsOpen, condition) |> unbox<IAnimatedTreeProp>
let CanHide (condition: bool) = (Keys.CanHide, condition) |> unbox<IAnimatedTreeProp>
let IsVisible (condition: bool) = (Keys.Visible, condition) |> unbox<IAnimatedTreeProp>
let EyeClicked (handler: unit -> unit) = (Keys.OnClick, handler) |> unbox<IAnimatedTreeProp>
let Children (children: ReactElement list) = (Keys.Children, children) |> unbox<IAnimatedTreeProp>
let Key (n: int) = ("key", n) |> unbox<IAnimatedTreeProp>
let Class (className: string) = (Keys.Class, className) |> unbox<IAnimatedTreeProp>
let Style (styles: CSSProp list) = (Keys.Style, keyValueList CaseRules.LowerFirst styles) |> unbox<IAnimatedTreeProp>
let Id (id: string) = (Keys.Id, id) |> unbox<IAnimatedTreeProp>
let OnToggled (handler: bool -> unit) = (Keys.Toggled, handler) |> unbox<IAnimatedTreeProp>

[<Emit "$2[$0] = $1">]
let private setProp (name: string) (value: obj) (result: obj) : unit = jsNative

let private buildConfig (props: (Keys * obj) list) =
  let config = obj()
  let modified =
    props
    |> List.distinctBy fst
    |> List.filter (fun (key, _) -> key <> Keys.Children)
  for (key, value) in modified do setProp (unbox key) value config
  config

let private createElement (el: obj, config: obj, children: ReactElement array) = import "createElement" "react"
let private nativeAnimatedTree : obj = importDefault "./animated-tree.js"

let animatedTree (props: IAnimatedTreeProp list) : ReactElement =
  (props |> unbox<(Keys * obj) list>)
  |> List.tryFind (fun (key, _) -> key = Keys.Children)
  |> function
    | None ->
        let config = buildConfig (props |> unbox)
        createElement(nativeAnimatedTree, config, null)
    | Some (_, children) ->
        let config =  buildConfig (props |> unbox)
        createElement(nativeAnimatedTree, config, Array.ofList (unbox<ReactElement list> children))
