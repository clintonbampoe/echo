namespace Echo.Seeder.Simulation;

/// <summary>
/// One kind of project a congregation raises money for.
/// </summary>
/// <param name="Name">What the project is called.</param>
/// <param name="Category">The project category it belongs to.</param>
/// <param name="MinMonths">
/// The cheapest it could be, in months of the congregation's tithe income.
/// This keeps a village church's borehole and a megachurch's auditorium the
/// right size for the people paying.
/// </param>
/// <param name="MaxMonths">The most expensive it could be, in months of tithe income.</param>
/// <param name="Description">One line shown in the project's details.</param>
public sealed record ProjectKind(
    string Name,
    string Category,
    double MinMonths,
    double MaxMonths,
    string Description
);

public static class ProjectCatalog
{
    public static IReadOnlyList<ProjectKind> All { get; } =
    [
        new(
            "Sanctuary Expansion",
            "Construction",
            4,
            10,
            "Extending the sanctuary to seat more worshippers."
        ),
        new(
            "New Auditorium",
            "Construction",
            6,
            12,
            "Building a new auditorium for services and conferences."
        ),
        new(
            "Church Annex Block",
            "Construction",
            2,
            6,
            "An annex for Sunday school classes and offices."
        ),
        new("Perimeter Wall", "Construction", 1, 3, "Fencing the church land to secure it."),
        new(
            "Roofing Replacement",
            "Construction",
            1,
            3,
            "Replacing the leaking roof sheets over the main hall."
        ),
        new("Classroom Block", "Construction", 3, 7, "Classrooms for the church school."),
        new(
            "Land Acquisition",
            "Land Acquisition",
            5,
            12,
            "Buying land for the congregation's permanent site."
        ),
        new(
            "Mission House Plot",
            "Land Acquisition",
            2,
            5,
            "A plot for a house for visiting missionaries."
        ),
        new("Sound System Upgrade", "Equipment", 0.3, 1, "New speakers, mixer and microphones."),
        new(
            "Generator Purchase",
            "Equipment",
            0.2,
            0.6,
            "A standby generator for services during power cuts."
        ),
        new(
            "Air Conditioning Installation",
            "Equipment",
            0.4,
            1.2,
            "Cooling for the main auditorium."
        ),
        new(
            "Projector and Screens",
            "Equipment",
            0.1,
            0.3,
            "Projectors and screens for lyrics and sermons."
        ),
        new(
            "Solar Installation",
            "Equipment",
            0.5,
            1.5,
            "Solar panels and batteries to cut the electricity bill."
        ),
        new(
            "Church Bus Purchase",
            "Transportation",
            1,
            3,
            "A bus for outreach and members without transport."
        ),
        new(
            "Outreach Van",
            "Transportation",
            0.8,
            2,
            "A van for carrying equipment and teams to outreaches."
        ),
        new(
            "Community Outreach Drive",
            "Outreach",
            0.1,
            0.4,
            "Food and clothing for needy families in the community."
        ),
        new(
            "Medical Outreach",
            "Outreach",
            0.2,
            0.6,
            "Free health screening and medicine for the community."
        ),
        new(
            "Borehole for the Community",
            "Outreach",
            0.3,
            0.8,
            "A borehole for a neighbouring community without clean water."
        ),
        new("Missions Fund", "Missions", 0.3, 1, "Supporting the congregation's missionaries."),
        new("New Church Plant", "Missions", 1, 3, "Starting a new branch in a nearby town."),
        new(
            "Scholarship Fund",
            "Education",
            0.2,
            0.8,
            "School fees for children of members in need."
        ),
        new("Library Project", "Education", 0.1, 0.4, "Books and furniture for a church library."),
        new(
            "Widows Support Fund",
            "Welfare",
            0.1,
            0.4,
            "Monthly support for widows in the congregation."
        ),
        new("Hospital Bills Fund", "Welfare", 0.1, 0.5, "Helping members with medical bills."),
        new(
            "Care Package Drive",
            "Welfare",
            0.05,
            0.2,
            "Festive care packages for families in need."
        ),
        new("Youth Center Build", "Youth", 1.5, 4, "A hall and recreation space for the youth."),
        new("Youth Camp", "Youth", 0.1, 0.4, "The annual camp for teenagers and young adults."),
        new(
            "Livestream Equipment",
            "Media",
            0.2,
            0.6,
            "Cameras and encoders to stream services online."
        ),
    ];
}
