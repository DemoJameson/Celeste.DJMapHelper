local colorGradeTrigger = {}

colorGradeTrigger.name = "DJMapHelper/colorGradeTrigger"
colorGradeTrigger.placements = {
    name = "normal",
    data = {
        ColorGrade = "none",
    }
}
colorGradeTrigger.fieldInformation = {
    ColorGrade = {
        fieldType = "anything",
        options = {
            "none",
            "cold",
            "credits",
            "feelingdown",
            "golden",
            "hot",
            "oldsite",
            "panicattack",
            "reflection",
            "templevoid"
        },
        editable = true
    }
}
return colorGradeTrigger