# What’s New & Fixed in v1.0.2

## Alterra Hub Mod Suite Fixes

Alterra Hub Mod Suite v1.0.2 is a small patch which contains fixes and changes for getting things ready for future updates and features.

## Added Features

- Juke Box
- Juke Box Wall Speaker
- Juke Box Sub Woofer



## Bug Fixed

- Alterra Storage Solutions: Server rack allowing you to remove server or transceiver from the rack when inventory of the player is full.  



# What’s New & Changed in v1.0.1

## Alterra Hub Mod Suite Fixes & Tuning Update

Alterra Hub Mod Suite v1.0.1 contains numerous fixes and small changes to functions that are the bread-and-butter of all minor updates, along with some changes in function to some objects. Some function changes were driven by technical necessity, some by player-interaction, and others from player feedback in tuning.


##### PRE-UPDATE RECOMMENDATIONS:
- Remove All Telepower Pylons
- Remove All Seabreeze Units
- Remove All Replicators
- Remove All Hydroponic Harvesters
- Remove All Universal Chargers
- Stop all processing in AutoCrafters


### AutoCrafter (Behavior Addition)
Continuous Operation is now broken down into two goals: infinite operation (e.g., Metal Salvage -> Titanium) and maintain-quantity operation (e.g., Keep an Inventory of 10 Lubricant). 

This is a side-ways addressment of some requests for Data Disk filtering (so as not to flood the storage) and to address the all-storage-options-base-flooding problem, as the AutoCrafter can utilize storage lockers, DSS, and other storage options.

“Standby” mode removed and replaced by “Assist” mode.
“Assisting” Units are chosen at programming time by the controlling Crafter.

AutoCrafters are no longer put in Standby for other crafters, they are merely idle. When setting up a program in an AutoCrafter, the player may select other AutoCrafters to “Assist” and those AutoCrafters will be slaved to the originating AutoCrafter (the parent) and their settings dialog will be inaccessible until released form “Assisting” the originating AutoCrafter.

This put all of the control data in one location, streamlines multi-AC programming, and simplifies the workflow associated with multi-AC operations during and after.


### Telepower Pylons (Behavior Change)
- Previously: a Base could have Pylons Pull power and Push power
- New Behavior: a Base may Pull power or Push power, not both

The game did not like it when power-loops were created (inducing hyper-lag and eventual crash) and while loops could be detected and prevented, the information available to the player to prevent, debug, or rearchitect was not available in a way that was useful to all users. 

To address this, Telepower Pylon architecture options were simplified such that if multiple Telepower Pylons are placed on a Base, they will synchronize. Changes made to one Pylon will be copied by all other Pylons on the base.

The result of this change is that a base can Push power or Pull power, not both.


### AlterraGen (Tuning & Behavior Changes)
As a result of tuning feedback and balancing related items in the Alterra Hub Energy Catalog, the following changes were made:

- Increased energy output, from 50epm to 70epm (0.833eps to 1.167eps)
- Battery increased to 1000 (due to JetStream @ 500 battery)
- Energy for Large Items (>1x1) is extracted more efficiently (x2.2)

The AlterraGen is a carry-forward from the stand-alone FCStudios mods and has several functional advantages over the stock bio-reactor, with reduced size, indoor/outdoor use, and no dependency on a multi-purpose room, however: player-feedback suggested a bumped power output; relative-balancing required a relative increase in battery; numerical analysis suggested adjustments to unattended-standing-delivery and leveraging the ‘bin vs slot’ design.

The energy bump and increased battery makes the AlterraGen separate it from the in-game bioreactor, letting them fulfill slightly different roles and objectives while also making the AlterraGen part of an upgrade-path in the bioreactor niche, rather than a simple alternative.

The energy-delivery from large (non-single-slot) items opens up new energy sources rather than defaulting to an oculus for energy efficiency due its overpowered energy-per-slot rating. To give some examples, a Bloodoil is 3x3 item yielding 420 energy in a stock bioreactor while in an AlterraGen it will yield 924 energy, an ampeel at a whopping 4x4 will net you 770 in a stock bioreactor but 1,694 in an AlterraGen.

This opens up new power sources and, if you’re brave enough to raise some large creatures, substantially increase the unattended-standing-delivery.


### WindSurfer (Tuning)
As a result of fixing a power-output bug and balancing related items in the Alterra Hub Energy Catalog, the following changes were made:

- Increase power-output to 7.67  energy per sec (up 2.88x from original)
- Increase battery to 1100 per turbine unit (up from 500)

The WindSurfer operation was intended to use the JetStreamT242 as a reference model but the output data was errantly entered, resulting in half its designed output.

Modeling the output full range of the JetStreamT242 and adjusting for other design objectives (e.g., offering honest choices) and abilities of the WindSurfer system itself (not just the turbine energy output), the energy output was increased to the 75th percentile of the JetStreamT242 absolute range. This means that while 2 optimally placed JetStreamT242 Marine Turbines can outperform 1 WindSurfer Turbine, that optimal/near-optimal placement is tricky and often dangerous, requires additional infrastructure and power transport, and scales very sub-optimally in effort-vs-reward relative to adding turbines. In other words: you can outperform the WindSurfer with 30 JetstreamT242 Turbines with the application of sweat-equity and ingenuity. 


### Solar Cluster (Tuning)
As a result of player-feedback and correcting some adaptive-design-elements, the following changes were made:

- Reduce battery to 350
- Reduce output to 10eps
- Price now 450,000

Realizing this will go over like a lead balloon to some, these changes needed to be made to correct some mistakes in the design process, change the biasing priorities to player expectation, and try to normalize the Energy Solutions Catalog.

The original design of the SolarCluster performance envelope was “13 solar panels”. In order to retain usage paradigms developed from the stand-along mods with some of the new objects in the new update, the decision was made to massively increase the battery of the Solar Cluster. When the object that was balanced against was completely reworked and split into multiple objects, removing that additional battery capacity was neglected, resulting in the Solar Cluster being m-m-massively overpowered.

And to be fair, even if it hadn’t been, a 975 battery was simply too much for a single, solar-niche product. (Please Note: the PowerStorage unit has been adjusted to better work with the Solar Cluster’s massive output. See PowerStorage)

The pricing model for objects was created by an algorithm and then referenced against materials return. Due to the simple materials of the solar panel vs the output, the Solar Cluster was priced very, very cheaply. This was noted at the time but the decision was made to stay close to the algorithm and reference biasing. Players, however, disagreed. Therefore, we created a pricing range of credit-to-energy-per-second and modeled to that to create a more normalized pricing in the Energy Solutions Catalog while still adjusting for other factors.

The reduction in power from 13 to 10 solar panels comes from player feedback tempered with relative balancing the other products in the Energy Solutions Catalog. 


### PowerStorage (Tuning)
As a result of player-feedback and the changes made to the SolarCluster, the following changes were made:

- Charging Range increased from 1 to 10eps to 1 to 50eps

This change was made to better deal with multiple, strong power sources providing a great deal of excess energy in times of plenty, with special note of the Solar Cluster, where one of these devices could overflow a single PowerStorage. Now the PowerStorage can handle the charging output of multiple Solar Clusters.


### Deep Drill: Heavy Duty (Tuning)
As a result of player-feedback, the following changes have been made:

- Default Ores-Per-Day: 25 (up from 12)
- Maximum Ores-Per-Day: 100 (down from infinite)
- Energy Consumption chart changed to an exponential scale after 25 ores-per-day: [HD Drill Energy Utilization Graph](https://docs.google.com/spreadsheets/d/1T7HeySGInp2m9wPkkj9K84r1LRYkrYRRYzGH2460GHA/edit#gid=0)
- Lubricant Usage now dependant on ores-per-day: [HD Drill Lubricant Utilization Graph](https://docs.google.com/spreadsheets/d/1mU_36Awb81XXkSHL2xx77nB7I6adOD3IiiCs5Dsgi6g/edit#gid=1573330764)

Note that energy consumption and lubricant usage are unchanged at the default ores-per-day: 2.66 energy per second and it takes 30 game-days to empty a full lubricant tank.

### Deep Drill: Heavy and Light Duty (Tuning)
Drills can be turned on and off.


### Replicator & Hydroponic Harvester (Behavior Change)
The “Remove Sample” button will remove the programming for a chamber but only if it is empty. To purge the inventory, click-and-hold the “Remove Sample” button and the inventory will be purged/destroyed and the programming cleared. 


### DSS C48 Panel  & EasyCraft Support (Tuning)
Seabreeze units can be seen by DSS. Seabreeze, Replicator, and Hydroponic Harvester have their own filters.

Seabreeze, Replicator, and Hydroponic Harvester are also reachable by EasyCraft.


## Bug Fixed, Exterminated, Banished, and Otherwise Chastised

- Autocrafter could not make standard air tank for high-capacity tank recipe
- Universal Charger would not return PowerCells (or accept via drag/drop)
- Fixes for Transport Drone becoming stuck mid-delivery 
- ResetTransportDrones strengthened and refunds canceled pending order
- Transport Drone no longer affected by propulsion cannon
- Fire Extinguisher no longer disappears from Refueler
- Transceiver help text adjusted
- When Autocrafter makes Banners from the Custom Banners mod, they are ? items
- Heads-Up Power Display always reads 0.0
- DSS no longer counts items in "containers" it cannot fulfill
- Message for drone xfer to hub no longer says "being shipped" when delivering a shipment
- 'Standby' Autocrafter no longer gives bonus item
- Harvester: tiger plants are no longer aggressive and brain coral no longer blow air bubbles
- Recycler: **regression**: items cannot be put directly into the recycler
- "Encyclopedia" title is misspelled on PDA Encyclopedia
- Transceiver dialog field "...storage has [number] of the selected..." does not display saved/understood values correctly
- Light Duty Drill Item Filter does not reset biome
- Editable items grown in the hydroponic harvester but taken out by the C48 or S23 panel may not be fresh as they are when pulled directly from the harvester
- "None" for recipe for the following kits: Base Oxygen Tank, Remote Storage, DSS Terminals, Antenna, Server Racks, Hydroponic Harvester, Matter Analyzer, Drills, Autocrafter, Trash Receptacle, Recycler, LED lights.
- Telepower Pylon: Looping power (A+B Push, A receive B, B receive A) causes hyperlag and possible CTD 
- Transceiver Settings redesigned
- Various PowerStorage issues straightened out
- Autocrafter: "recursive" renamed "continuous" (and there was great rejoicing)
- Currency Icon now next to Currency in the shopping cart
- Short LED Light Stick has a ghost again (spooky, spooky) and can be placed (regression fix)
- Transceiver has stopped performing miracles on transferred items (regression fix)
- C48 reverts to first page of inventory when items added
- (Hardcore Game Mode Only) Matter Analyzer: DNA samples scanned are lost between save/reload.
- Matter Analyzer does not accept Grub Basket (bug) or Creep Vine Sample (change)
- Light Duty Drill: not showing biome.
- Stove returns a max of 16 cooked items but more items can be input
- Microwave cooks your food, eats it, then looks at you all innocent.
- Receptacle no longer errantly accepts Depleted Reactor Rod
- Hydroponic Harvester: If the same item is in multiple bays, one bay cannot be cleared until all bays of that item are cleared.
- Heavy Duty Drill, Base Connection intermittent or will not connect to base even when in range.
- Building the **WIndSurfer Operator** will reset your AlterraCredit to the value held when the savegame was loaded.
- Ores spawning after setting up automation for Heavy Duty drill
- SeaBreeze: items put in the SeaBreeze duplicate in-world; may appear outside the base or when the SeaBreeze is deconstructed
- Automatic Debt Payment takes the % of payment but doesn't apply it. 
- PDA forgets you already have an Alterra Account.
- PowerStorage not charging from the base it is placed on
- Telepower Pylon screen now reads "Select a Mode or Connect to another Pylon"
- The overlay tooltip for the "?" double-counts items in Remote Storage as being in Storage Lockers.
- Confusing text for the "?" tooltip replaced (adding the unit " Items " after each count to avoid confusion
- "?" Tooltip counts raw items, not x/y for consistency with lockers
- Base Oxygen Tank Tooltip more descriptive for pre-PDA use
- Peeper Lounge SFX toggle works for all audio.
- Base ID as well as base Name are displayed on the FCS PDA homescreen.
- Control Room terminals now "emergency" instead of "emergancy"
- Two options added under Options> Mods> Alterra Hub for control of on-screen messages: "Show Credit Messages (default ON)" and Hide All F.C.S On-Screen Messages (default OFF)"
- Remove Hardcore function from Drills
- WindSurfer no longer eats kits when it can no longer build more modules