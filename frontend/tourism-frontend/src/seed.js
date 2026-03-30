/**
 * Tourism App - MongoDB Seed Script
 * 
 * Seeds all three databases:
 *   - auth-db   (users)
 *   - blog-db   (blogs, comments, likes)
 *   - tour-db   (tours, keypoints, carts, tokens, executions)
 * 
 * Run with:
 *   node seed.js
 * 
 * Requirements:
 *   npm install mongodb bcryptjs
 */

const { MongoClient, ObjectId } = require("mongodb");
const bcrypt = require("bcryptjs");

const MONGO_URI = "mongodb://localhost:27017";

// ─── Helpers ────────────────────────────────────────────────────────────────

function id() {
  return new ObjectId();
}

function daysAgo(n) {
  const d = new Date();
  d.setDate(d.getDate() - n);
  return d;
}

// ─── Fixed IDs (so relationships work across collections) ───────────────────

// Users
const adminId    = id();
const guide1Id   = id();
const guide2Id   = id();
const tourist1Id = id();
const tourist2Id = id();
const tourist3Id = id();

// Tours
const tour1Id = id();
const tour2Id = id();
const tour3Id = id();

// KeyPoints
const kp1_1Id = id();
const kp1_2Id = id();
const kp1_3Id = id();
const kp2_1Id = id();
const kp2_2Id = id();
const kp3_1Id = id();
const kp3_2Id = id();

// Blogs
const blog1Id = id();
const blog2Id = id();
const blog3Id = id();
const blog4Id = id();

// ─── Auth DB ─────────────────────────────────────────────────────────────────

async function seedAuthDb(client) {
  const db = client.db("auth-db");
  const users = db.collection("users");

  await users.deleteMany({});

  const hash = (pw) => bcrypt.hashSync(pw, 10);

  const docs = [
    {
      _id: adminId,
      username: "admin",
      email: "admin@tourism.com",
      passwordHash: hash("Admin123!"),
      role: "admin",
      isBlocked: false,
      profile: {
        firstName: "System",
        lastName: "Administrator",
        profilePicture: "",
        bio: "Platform administrator.",
        motto: "Keeping things running.",
      },
      createdAt: daysAgo(60),
    },
    {
      _id: guide1Id,
      username: "marko_guide",
      email: "marko@tourism.com",
      passwordHash: hash("Guide123!"),
      role: "guide",
      isBlocked: false,
      profile: {
        firstName: "Marko",
        lastName: "Petrović",
        profilePicture: "",
        bio: "Experienced guide specializing in mountain tours across Serbia.",
        motto: "Every trail tells a story.",
      },
      createdAt: daysAgo(45),
    },
    {
      _id: guide2Id,
      username: "ana_guide",
      email: "ana@tourism.com",
      passwordHash: hash("Guide123!"),
      role: "guide",
      isBlocked: false,
      profile: {
        firstName: "Ana",
        lastName: "Jovanović",
        profilePicture: "",
        bio: "City tour expert. I know every corner of Belgrade and Novi Sad.",
        motto: "Cities have souls — let me show you theirs.",
      },
      createdAt: daysAgo(40),
    },
    {
      _id: tourist1Id,
      username: "nikola_tourist",
      email: "nikola@tourism.com",
      passwordHash: hash("Tourist123!"),
      role: "tourist",
      isBlocked: false,
      profile: {
        firstName: "Nikola",
        lastName: "Stanković",
        profilePicture: "",
        bio: "Avid hiker and travel blogger.",
        motto: "Not all who wander are lost.",
      },
      createdAt: daysAgo(30),
    },
    {
      _id: tourist2Id,
      username: "jelena_tourist",
      email: "jelena@tourism.com",
      passwordHash: hash("Tourist123!"),
      role: "tourist",
      isBlocked: false,
      profile: {
        firstName: "Jelena",
        lastName: "Nikolić",
        profilePicture: "",
        bio: "Weekend explorer. Coffee and culture enthusiast.",
        motto: "Life is short, travel more.",
      },
      createdAt: daysAgo(25),
    },
    {
      _id: tourist3Id,
      username: "stefan_tourist",
      email: "stefan@tourism.com",
      passwordHash: hash("Tourist123!"),
      role: "tourist",
      isBlocked: false,
      profile: {
        firstName: "Stefan",
        lastName: "Đorđević",
        profilePicture: "",
        bio: "Photography enthusiast documenting hidden gems of the Balkans.",
        motto: "Capture the moment.",
      },
      createdAt: daysAgo(20),
    },
  ];

  await users.insertMany(docs);
  console.log(`✅ auth-db: inserted ${docs.length} users`);
  console.log("   Credentials (all share same password pattern):");
  console.log("   admin          / Admin123!");
  console.log("   marko_guide    / Guide123!");
  console.log("   ana_guide      / Guide123!");
  console.log("   nikola_tourist / Tourist123!");
  console.log("   jelena_tourist / Tourist123!");
  console.log("   stefan_tourist / Tourist123!");
}

// ─── Blog DB ──────────────────────────────────────────────────────────────────

async function seedBlogDb(client) {
  const db = client.db("blog-db");

  // ── Blogs ──
  await db.collection("blogs").deleteMany({});
  const blogs = [
    {
      _id: blog1Id,
      userId: tourist1Id.toString(),
      username: "nikola_tourist",
      title: "Hiking Tara Mountain — A Weekend to Remember",
      description:
        "Spent the weekend on Tara and it was absolutely breathtaking. The pine forests, the Drina canyon views, and the silence — completely worth the 4-hour drive from Belgrade. If you haven't been, put it on your list immediately. We covered about 18km on Saturday and found a hidden waterfall that isn't on any map.",
      images: [],
      createdAt: daysAgo(10),
      likeCount: 5,
      commentCount: 2,
    },
    {
      _id: blog2Id,
      userId: tourist2Id.toString(),
      username: "jelena_tourist",
      title: "Belgrade Fortress at Golden Hour",
      description:
        "Took the Ana's city tour last week and the highlight was definitely ending at Kalemegdan right at sunset. The guide knows exactly where to stand to get that perfect view of the Sava and Danube meeting point. Highly recommend booking it — the historical context she provides makes the whole experience so much richer.",
      images: [],
      createdAt: daysAgo(7),
      likeCount: 8,
      commentCount: 3,
    },
    {
      _id: blog3Id,
      userId: tourist1Id.toString(),
      username: "nikola_tourist",
      title: "Đavolja Varoš — Devil's Town is Unreal",
      description:
        "I've seen photos of Đavolja Varoš a hundred times but nothing prepares you for seeing the rock formations in person. Over 200 stone pillars, some up to 15 meters tall. The guided tour explains the geological history and local legend. We also stopped at the two springs — the water tastes absolutely terrible (highly acidic) but you have to try it.",
      images: [],
      createdAt: daysAgo(5),
      likeCount: 12,
      commentCount: 1,
    },
    {
      _id: blog4Id,
      userId: tourist3Id.toString(),
      username: "stefan_tourist",
      title: "Novi Sad Photography Walk — Best Spots",
      description:
        "Did a solo photography walk through Novi Sad's old town and Petrovaradin fortress. The fortress offers an insane panorama of the city and the Danube. Best time for photos is early morning when there are no crowds. The old town streets around Zmaj Jovina are also incredibly photogenic — old architecture, colorful facades, cats everywhere.",
      images: [],
      createdAt: daysAgo(3),
      likeCount: 6,
      commentCount: 2,
    },
  ];
  await db.collection("blogs").insertMany(blogs);

  // ── Comments ──
  await db.collection("comments").deleteMany({});
  const comments = [
    {
      _id: id(),
      blogId: blog1Id,
      userId: tourist2Id.toString(),
      username: "jelena_tourist",
      text: "I was on Tara last summer! Did you go through Mitrovac? The views from there are unreal.",
      createdAt: daysAgo(9),
      lastModified: daysAgo(9),
    },
    {
      _id: id(),
      blogId: blog1Id,
      userId: tourist3Id.toString(),
      username: "stefan_tourist",
      text: "Great shots! Which trail did you take for the waterfall?",
      createdAt: daysAgo(8),
      lastModified: daysAgo(8),
    },
    {
      _id: id(),
      blogId: blog2Id,
      userId: tourist1Id.toString(),
      username: "nikola_tourist",
      text: "Ana's tours are the best. She knows so much history about the fortress.",
      createdAt: daysAgo(6),
      lastModified: daysAgo(6),
    },
    {
      _id: id(),
      blogId: blog2Id,
      userId: tourist3Id.toString(),
      username: "stefan_tourist",
      text: "Golden hour there is magical. I went for the sunrise once — even better.",
      createdAt: daysAgo(6),
      lastModified: daysAgo(6),
    },
    {
      _id: id(),
      blogId: blog2Id,
      userId: tourist2Id.toString(),
      username: "jelena_tourist",
      text: "Just booked the tour for next Saturday, can't wait!",
      createdAt: daysAgo(5),
      lastModified: daysAgo(5),
    },
    {
      _id: id(),
      blogId: blog3Id,
      userId: tourist2Id.toString(),
      username: "jelena_tourist",
      text: "The acidic spring water is genuinely the worst thing I've ever tasted. 10/10 would try again.",
      createdAt: daysAgo(4),
      lastModified: daysAgo(4),
    },
    {
      _id: id(),
      blogId: blog4Id,
      userId: tourist1Id.toString(),
      username: "nikola_tourist",
      text: "The cats in Novi Sad's old town are an institution. Great photos!",
      createdAt: daysAgo(2),
      lastModified: daysAgo(2),
    },
    {
      _id: id(),
      blogId: blog4Id,
      userId: tourist2Id.toString(),
      username: "jelena_tourist",
      text: "I need to do this walk. Adding it to my list!",
      createdAt: daysAgo(1),
      lastModified: daysAgo(1),
    },
  ];
  await db.collection("comments").insertMany(comments);

  // ── Likes ──
  await db.collection("likes").deleteMany({});
  const likes = [
    // blog1 — 5 likes
    { _id: id(), blogId: blog1Id, userId: tourist2Id.toString(), createdAt: daysAgo(9) },
    { _id: id(), blogId: blog1Id, userId: tourist3Id.toString(), createdAt: daysAgo(9) },
    { _id: id(), blogId: blog1Id, userId: guide1Id.toString(),   createdAt: daysAgo(8) },
    { _id: id(), blogId: blog1Id, userId: guide2Id.toString(),   createdAt: daysAgo(8) },
    { _id: id(), blogId: blog1Id, userId: adminId.toString(),    createdAt: daysAgo(7) },
    // blog2 — 8 likes
    { _id: id(), blogId: blog2Id, userId: tourist1Id.toString(), createdAt: daysAgo(6) },
    { _id: id(), blogId: blog2Id, userId: tourist3Id.toString(), createdAt: daysAgo(6) },
    { _id: id(), blogId: blog2Id, userId: guide1Id.toString(),   createdAt: daysAgo(5) },
    { _id: id(), blogId: blog2Id, userId: guide2Id.toString(),   createdAt: daysAgo(5) },
    { _id: id(), blogId: blog2Id, userId: adminId.toString(),    createdAt: daysAgo(5) },
    { _id: id(), blogId: blog2Id, userId: tourist2Id.toString(), createdAt: daysAgo(4) },
    { _id: id(), blogId: blog2Id, userId: tourist1Id.toString(), createdAt: daysAgo(4) },
    { _id: id(), blogId: blog2Id, userId: tourist3Id.toString(), createdAt: daysAgo(3) },
    // blog3 — 12 likes
    { _id: id(), blogId: blog3Id, userId: tourist1Id.toString(), createdAt: daysAgo(4) },
    { _id: id(), blogId: blog3Id, userId: tourist2Id.toString(), createdAt: daysAgo(4) },
    { _id: id(), blogId: blog3Id, userId: tourist3Id.toString(), createdAt: daysAgo(3) },
    { _id: id(), blogId: blog3Id, userId: guide1Id.toString(),   createdAt: daysAgo(3) },
    { _id: id(), blogId: blog3Id, userId: guide2Id.toString(),   createdAt: daysAgo(3) },
    { _id: id(), blogId: blog3Id, userId: adminId.toString(),    createdAt: daysAgo(2) },
    // blog4 — 6 likes
    { _id: id(), blogId: blog4Id, userId: tourist1Id.toString(), createdAt: daysAgo(2) },
    { _id: id(), blogId: blog4Id, userId: tourist2Id.toString(), createdAt: daysAgo(2) },
    { _id: id(), blogId: blog4Id, userId: guide1Id.toString(),   createdAt: daysAgo(1) },
    { _id: id(), blogId: blog4Id, userId: guide2Id.toString(),   createdAt: daysAgo(1) },
    { _id: id(), blogId: blog4Id, userId: adminId.toString(),    createdAt: daysAgo(1) },
    { _id: id(), blogId: blog4Id, userId: tourist3Id.toString(), createdAt: daysAgo(1) },
  ];
  await db.collection("likes").insertMany(likes);

  console.log(`✅ blog-db: inserted ${blogs.length} blogs, ${comments.length} comments, ${likes.length} likes`);
}

// ─── Tour DB ──────────────────────────────────────────────────────────────────

async function seedTourDb(client) {
  const db = client.db("tour-db");

  // ── Tours ──
  await db.collection("tours").deleteMany({});
  const tours = [
    {
      _id: tour1Id,
      guideId: guide1Id.toString(),
      name: "Tara Mountain Adventure",
      description:
        "A full-day hiking tour through the stunning Tara National Park. We cover the best viewpoints over the Drina canyon, visit the hidden Beli Rzav waterfall, and walk through ancient pine forests. Suitable for intermediate hikers.",
      difficulty: "medium",
      tags: ["hiking", "nature", "mountains", "waterfall"],
      status: "published",
      price: 25.0,
      isPublished: true,
      publishedAt: daysAgo(30),
      createdAt: daysAgo(35),
    },
    {
      _id: tour2Id,
      guideId: guide2Id.toString(),
      name: "Belgrade Historical Walk",
      description:
        "Explore the heart of Belgrade on foot. Starting from Republic Square, we walk through Knez Mihailova street, visit Kalemegdan Fortress, and end with a panoramic view of the confluence of the Sava and Danube rivers at golden hour.",
      difficulty: "easy",
      tags: ["history", "culture", "city", "architecture"],
      status: "published",
      price: 15.0,
      isPublished: true,
      publishedAt: daysAgo(20),
      createdAt: daysAgo(25),
    },
    {
      _id: tour3Id,
      guideId: guide1Id.toString(),
      name: "Đavolja Varoš — Devil's Town",
      description:
        "Visit one of Serbia's most unique natural landmarks. Over 200 earth pyramids formed by erosion, two acidic springs, and breathtaking highland scenery. This tour includes geological and historical explanations at every stop.",
      difficulty: "easy",
      tags: ["nature", "geology", "sightseeing", "unique"],
      status: "draft",
      price: 20.0,
      isPublished: false,
      publishedAt: new Date(0),
      createdAt: daysAgo(10),
    },
  ];
  await db.collection("tours").insertMany(tours);

  // ── KeyPoints ──
  await db.collection("keypoints").deleteMany({});
  const keypoints = [
    // Tour 1 — Tara Mountain
    {
      _id: kp1_1Id,
      tourId: tour1Id,
      name: "Kaluđerske Bare Viewpoint",
      description: "The starting point of the tour with a sweeping view of the Tara plateau.",
      latitude: 43.8912,
      longitude: 19.5233,
      image: "",
      order: 1,
    },
    {
      _id: kp1_2Id,
      tourId: tour1Id,
      name: "Beli Rzav Waterfall",
      description: "A hidden 12-meter waterfall deep in the pine forest. Bring a jacket — it's always cool here.",
      latitude: 43.8756,
      longitude: 19.5489,
      image: "",
      order: 2,
    },
    {
      _id: kp1_3Id,
      tourId: tour1Id,
      name: "Drina Canyon Viewpoint",
      description: "The best view of the Drina river canyon on the tour. 800 meters straight down.",
      latitude: 43.8634,
      longitude: 19.5612,
      image: "",
      order: 3,
    },
    // Tour 2 — Belgrade Walk
    {
      _id: kp2_1Id,
      tourId: tour2Id,
      name: "Republic Square",
      description: "Meeting point. The heart of Belgrade, dominated by the National Museum and the National Theatre.",
      latitude: 44.8176,
      longitude: 20.4569,
      image: "",
      order: 1,
    },
    {
      _id: kp2_2Id,
      tourId: tour2Id,
      name: "Kalemegdan Fortress",
      description: "The medieval fortress at the confluence of the Sava and Danube. Over 2,000 years of history on this spot.",
      latitude: 44.8234,
      longitude: 20.4489,
      image: "",
      order: 2,
    },
    // Tour 3 — Đavolja Varoš
    {
      _id: kp3_1Id,
      tourId: tour3Id,
      name: "Main Rock Formation Field",
      description: "The central field with the densest concentration of earth pyramids. The tallest here reaches 15 meters.",
      latitude: 43.0912,
      longitude: 21.3901,
      image: "",
      order: 1,
    },
    {
      _id: kp3_2Id,
      tourId: tour3Id,
      name: "The Acidic Springs",
      description: "Two natural springs with extremely acidic water (pH ~1.5). The source of the erosion that created the formations.",
      latitude: 43.0934,
      longitude: 21.3923,
      image: "",
      order: 2,
    },
  ];
  await db.collection("keypoints").insertMany(keypoints);

  // ── Purchase Tokens (tourist1 and tourist2 bought tour1; tourist1 also bought tour2) ──
  await db.collection("purchasetokens").deleteMany({});
  const tokens = [
    {
      _id: id(),
      touristId: tourist1Id.toString(),
      tourId: tour1Id,
      token: "TOKEN-T1-TOUR1-" + tourist1Id.toString().slice(-6).toUpperCase(),
      purchasedAt: daysAgo(15),
    },
    {
      _id: id(),
      touristId: tourist1Id.toString(),
      tourId: tour2Id,
      token: "TOKEN-T1-TOUR2-" + tourist1Id.toString().slice(-6).toUpperCase(),
      purchasedAt: daysAgo(12),
    },
    {
      _id: id(),
      touristId: tourist2Id.toString(),
      tourId: tour1Id,
      token: "TOKEN-T2-TOUR1-" + tourist2Id.toString().slice(-6).toUpperCase(),
      purchasedAt: daysAgo(8),
    },
    {
      _id: id(),
      touristId: tourist3Id.toString(),
      tourId: tour2Id,
      token: "TOKEN-T3-TOUR2-" + tourist3Id.toString().slice(-6).toUpperCase(),
      purchasedAt: daysAgo(5),
    },
  ];
  await db.collection("purchasetokens").insertMany(tokens);

  // ── Shopping Cart (tourist3 has tour1 in cart) ──
  await db.collection("shoppingcarts").deleteMany({});
  const carts = [
    {
      _id: id(),
      touristId: tourist3Id.toString(),
      items: [
        {
          tourId: tour1Id,
          tourName: "Tara Mountain Adventure",
          price: 25.0,
        },
      ],
      totalPrice: 25.0,
    },
  ];
  await db.collection("shoppingcarts").insertMany(carts);

  // ── Tour Executions ──
  await db.collection("tourexecutions").deleteMany({});
  const executions = [
    // tourist1 completed tour1
    {
      _id: id(),
      touristId: tourist1Id.toString(),
      tourId: tour1Id,
      status: "completed",
      startedAt: daysAgo(14),
      completedAt: daysAgo(14),
      lastActivity: daysAgo(14),
      startLatitude: 43.8912,
      startLongitude: 19.5233,
      completedKeypoints: [
        { keypointId: kp1_1Id, completedAt: daysAgo(14) },
        { keypointId: kp1_2Id, completedAt: daysAgo(14) },
        { keypointId: kp1_3Id, completedAt: daysAgo(14) },
      ],
    },
    // tourist1 has an active execution on tour2
    {
      _id: id(),
      touristId: tourist1Id.toString(),
      tourId: tour2Id,
      status: "active",
      startedAt: daysAgo(1),
      completedAt: new Date(0),
      lastActivity: daysAgo(1),
      startLatitude: 44.8176,
      startLongitude: 20.4569,
      completedKeypoints: [
        { keypointId: kp2_1Id, completedAt: daysAgo(1) },
      ],
    },
    // tourist2 abandoned tour1
    {
      _id: id(),
      touristId: tourist2Id.toString(),
      tourId: tour1Id,
      status: "abandoned",
      startedAt: daysAgo(7),
      completedAt: new Date(0),
      lastActivity: daysAgo(7),
      startLatitude: 43.8912,
      startLongitude: 19.5233,
      completedKeypoints: [
        { keypointId: kp1_1Id, completedAt: daysAgo(7) },
      ],
    },
  ];
  await db.collection("tourexecutions").insertMany(executions);

  // ── Positions ──
  await db.collection("positions").deleteMany({});
  const positions = [
    {
      _id: id(),
      touristId: tourist1Id.toString(),
      latitude: 44.8176,
      longitude: 20.4569,
      updatedAt: new Date(),
    },
    {
      _id: id(),
      touristId: tourist2Id.toString(),
      latitude: 43.8912,
      longitude: 19.5233,
      updatedAt: new Date(),
    },
    {
      _id: id(),
      touristId: tourist3Id.toString(),
      latitude: 45.2671,
      longitude: 19.8335,
      updatedAt: new Date(),
    },
  ];
  await db.collection("positions").insertMany(positions);

  console.log(
    `✅ tour-db: inserted ${tours.length} tours, ${keypoints.length} keypoints, ` +
    `${tokens.length} purchase tokens, ${carts.length} shopping carts, ` +
    `${executions.length} executions, ${positions.length} positions`
  );
}

// ─── Main ─────────────────────────────────────────────────────────────────────

async function main() {
  const client = new MongoClient(MONGO_URI);

  try {
    await client.connect();
    console.log("🔌 Connected to MongoDB\n");

    await seedAuthDb(client);
    await seedBlogDb(client);
    await seedTourDb(client);

    console.log("\n🎉 All databases seeded successfully!");
    console.log("\n📋 Summary:");
    console.log("   Users    : 6 (1 admin, 2 guides, 3 tourists)");
    console.log("   Tours    : 3 (2 published, 1 draft)");
    console.log("   KeyPoints: 7 across all tours");
    console.log("   Blogs    : 4 with comments and likes");
    console.log("   Tokens   : 4 (tourists have purchased tours)");
    console.log("   Cart     : 1 active cart (stefan_tourist)");
    console.log("   Sessions : 3 (1 completed, 1 active, 1 abandoned)");
    console.log("\n⚠️  Note: Follow relationships (Neo4j) must be created");
    console.log("   manually through the app — follow a few users after seeding.");
  } catch (err) {
    console.error("❌ Seed failed:", err);
    process.exit(1);
  } finally {
    await client.close();
  }
}

main();