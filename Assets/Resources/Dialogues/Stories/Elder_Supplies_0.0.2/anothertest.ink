VAR quest_foraging_started = false
VAR quest_foraging_complete = false
VAR can_complete_foraging_quest = false

-> start

=== start ===
{ quest_foraging_complete: -> quest_done }
{ quest_foraging_started && can_complete_foraging_quest: -> can_turn_in }
{ quest_foraging_started: -> quest_active }

"The forest yields less each day."

+ [Ask about work]
    "Food grows scarce. The usual paths... resist travel."
    
    * * [Accept]
        ~ quest_foraging_started = true
        # QUEST_START: foraging_mission
        "Ehehee..."
        -> END
        
    * * [Refuse]
        "Then we starve together."
        -> END

+ [Ask about the forest]
    "It watches now. Where once it merely grew."
    "It calls out for an end."
    -> start
    
+ [Leave]
    -> END

=== quest_active ===
"Still breathing. That's something."

+ [Report progress]
    "Continue. The forest will decide what it gives."
    -> END
    
+ [Ask for guidance]
    "Trust your hands. Your eyes lie more each day."
    -> END
    
+ [Mention strange sightings]
    "The trees remember what we've forgotten."
    -> END

=== can_turn_in ===
"Your pack is full. Good."

+ [Turn in supplies]
    ~ quest_foraging_complete = true
    
    "This will do. For now."
    
    "The forest gave willingly."
    -> END
    
+ [Not yet]
    "Hunger comes for us."
    -> END

=== quest_done ===
"The bellies are full. The forest still watches."

+ [Ask what's next]
    "We endure until the next hunger."
    -> END
    
+ [Ask about the forest's behavior]
    "Perhaps it's finally tired of us."
    -> END
    
+ [Leave]
    -> END