# GraphUI3

GraphUI3 is an app based on Graph Theory concepts, the app allows you to create, edit and run  visual algorithms on graphs.

For more information about Graph Theory, click [here](https://brilliant.org/wiki/graph-theory/).

## Installation
Simply download and install the app from the [Microsoft Store](https://microsoft.com) for the latest release.

If you want to open the app in Visual Studio Community / Pro / Enterprise and see how the code actually works, you'll need the [Windows App SDK](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment?tabs=cs-vs-community%2Ccpp-vs-community%2Cvs-2022-17-1-a%2Cvs-2022-17-1-b) and clone this repository.

# Usage

## 1. The Graphing Area & Components

### 1.1 Nodes

![image](https://github.com/roland31x/GraphUI3/assets/115028239/0b4ecd15-b8a0-41ed-831c-514f58be9437)

The fundamental unit of which graphs are formed.

>[!TIP]
> Create nodes by double clicking anywhere on the app.

>[!TIP]
> You can move a node around by moving the mouse while holding left click on a node.

Right clicking on a node brings up it's option menu where you can give it a name or delete it from your graph.

![image](https://github.com/roland31x/GraphUI3/assets/115028239/848d9b5c-aaf4-4771-a123-e571d07e545c)

>[!TIP]
> Double click on an existing node to select it (its border will turn red), a selected node can interact with another selected node.

![image](https://github.com/roland31x/GraphUI3/assets/115028239/ba4533dd-c488-4c7d-988a-9c7881487384)

##
### 1.2 Edges 

![image](https://github.com/roland31x/GraphUI3/assets/115028239/6bb00dac-81af-454a-ac1d-5e229f2446c2)

Edges link nodes together, their weight determines the "distance" between nodes.

> [!TIP]
> Create edges by selecting two distinct nodes.

> [!IMPORTANT]
> Loops are not yet supported, you cannot create an edge from the same node to the same node.

> [!IMPORTANT]
> So far, the app only supports unoriented graphs (WiP).

Right clicking on an edge brings up its options menu where you can change its weight or delete it from your graph.

![image](https://github.com/roland31x/GraphUI3/assets/115028239/036c38d1-acab-4a64-86a2-61d65c5d41e9)

> [!NOTE]
> Edge weights CANNOT be negative. If set to 0 their weight will not show up in the UI but they are treated as "1".

##
## 2. The App Bar & Title
![image](https://github.com/roland31x/GraphUI3/assets/115028239/af1c538a-4aa6-48fd-93bc-ecbde9a7aa38)

##
### 2.1. The 'File' menu 

Your basic file opening / saving / loading menu, you can choose to save your current graph to a file, load up another graph or create a new graph.

![image](https://github.com/roland31x/GraphUI3/assets/115028239/8bcdface-ab98-4560-82a5-7d2c009fb928)

> [!NOTE]
> If the graph is not saved the app will ask if you want to save your changes before discarding.

> [!CAUTION]
> It is not recommended to manually edit these graph files, it can lead to errors where the app cannot load up the graph!
> If you want to manually edit the graph please refer to the end of this readme to see how the files are actually formed.

##
### 2.2. The 'Raw' menu 

Contains your entire current graph data, it is exactly what is saved inside a graph file as text, you can opt to copy the contents directly from here if you want.

![image](https://github.com/roland31x/GraphUI3/assets/115028239/7626fa2c-2deb-44f5-ba81-4b15d4ac8594)

> [!NOTE]
> The graph is not editable from the 'Raw' menu yet.

##
### 2.3. The 'Edit' menu 

Contains various quick editing features of your current loaded graph.

![image](https://github.com/roland31x/GraphUI3/assets/115028239/0187f633-439a-464d-be05-b862b529b74c)

2.3.1. The difference between creating a new graph and resetting current is when you already have a graph loaded from file, resetting keeps file path, creating a new one will require you to assign a path to it when saving.

2.3.2. Resetting the colors doesn't affect your graphs traits. ( Colors are not saved , they are purely visual )

2.3.3. Resetting weights converts your current graph to a fully unweighed one.

2.3.4. Auto-Set Weights by Distance will convert your graph to a weighed one where the weights are the pixel distances between nodes. ( updated automatically )

> [!NOTE]
> In this app edge weight default values are 1, even if edge weight is 0 it is always considered 1 since there cannot be two or more nodes 0 distance apart.
> If you leave weights at 0 they will not show up on the UI, 1 will show up but they are treated as equal!
##
2.4 The 'Help' menu contains the button that brings you to this page :)

![image](https://github.com/roland31x/GraphUI3/assets/115028239/737c980f-6566-4dad-a2f8-5f8e10ce7fc4)

##
## 3. The Algorithms

![image](https://github.com/roland31x/GraphUI3/assets/115028239/5f70190e-7956-4012-a578-2135821106ad)

*as of ver 1.0.0
##
### 3.1. [Coloring](https://en.wikipedia.org/wiki/Graph_coloring)
##
### 3.2. Elementary Paths

*The "First Path only" switch sets wether these algorithms should return the full list ( capped at 100 ) existent path ( there can be a lot of them ) or just stop at the first one found. ( to check for existance and visually show it )

3.2.1-3.2.2 [Hamilton Path / Cycle](https://en.wikipedia.org/wiki/Hamiltonian_path)

3.2.3-3.2.4 [Euler Path / Cycle](https://en.wikipedia.org/wiki/Eulerian_path)
##
### 3.3. Distance Paths (WiP)
##
### 3.4. Spanning Trees
3.4.1 [Minimum Spanning Tree](https://en.wikipedia.org/wiki/Minimum_spanning_tree)


## Contributing

Pull requests are welcome. I'll try my best to add as much interactive stuff to the app and make it robust. Hit me up with ideas.

## Advanced Stuff
